using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Exceptions;
using TrialWorld.Core.StreamInfo; // Assuming MediaInfo related types are here
using TrialWorld.Infrastructure.Models.FFmpeg;

namespace TrialWorld.Infrastructure.FFmpeg // Placing it here for now
{
    /// <summary>
    /// Implementation of <see cref="IMediaInfoService"/> using the ffprobe command-line tool.
    /// </summary>
    public class FFprobeMediaInfoService : IMediaInfoService
    {
        private readonly ILogger<FFprobeMediaInfoService> _logger;
        private readonly IProcessRunner _processRunner;
        private readonly string _ffprobePath;

        public FFprobeMediaInfoService(
            ILogger<FFprobeMediaInfoService> logger,
            IProcessRunner processRunner,
            IOptions<FFmpegOptions> options) // Assuming FFmpegOptions contains ffprobe path
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));

            if (options?.Value == null) throw new ArgumentNullException(nameof(options));
            _ffprobePath = options.Value.FFprobePath ?? "ffprobe";

            // Validate ffprobe path
            // Consider adding a check if the file actually exists, similar to FFmpegService constructor
            if (string.IsNullOrEmpty(_ffprobePath))
            {
                _logger.LogCritical("ffprobe path is not configured.");
                throw new ConfigurationValidationException("The path to ffprobe is not configured in FFmpegOptions.");
            }
        }

        /// <inheritdoc />
        public async Task<MediaInfo> GetMediaInfoAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                _logger.LogError("GetMediaInfoAsync called with null or empty file path.");
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                _logger.LogError("Media file not found at path: {FilePath}", filePath);
                throw new FileNotFoundException("The specified media file does not exist.", filePath);
            }

            // Build FFprobe arguments to get media information in JSON format
            var arguments = $"-v quiet -print_format json -show_format -show_streams \"{filePath}\"";
            _logger.LogInformation("Probing media file: {FilePath}", filePath);

            try
            {
                // Run FFprobe process using IProcessRunner
                _logger.LogDebug("Running ffprobe with arguments: {Arguments}", arguments);
                var output = await _processRunner.RunProcessAsync(_ffprobePath, arguments, cancellationToken);

                if (string.IsNullOrWhiteSpace(output))
                {
                    _logger.LogWarning("ffprobe returned empty output for file: {FilePath}", filePath);
                    throw new InvalidOperationException("ffprobe returned empty output.");
                }

                _logger.LogDebug("ffprobe output received: {OutputLength} characters", output.Length);

                // Parse the JSON output
                var probeData = JsonSerializer.Deserialize<FFprobeOutput>(output, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (probeData == null)
                {
                    _logger.LogError("Failed to parse ffprobe JSON output for file: {FilePath}", filePath);
                    throw new InvalidOperationException("Failed to parse FFprobe output.");
                }

                 _logger.LogInformation("Successfully probed media file: {FilePath}", filePath);
                // Convert FFprobe output to MediaInfo
                return ConvertToMediaInfo(probeData, filePath);
            }
            catch (ExternalProcessException ex)
            {
                _logger.LogError(ex, "ffprobe process failed for file {FilePath}. ExitCode={ExitCode}", filePath, ex.ExitCode);
                throw new InvalidOperationException($"Failed to probe media file using ffprobe (ExitCode: {ex.ExitCode}). Error: {ex.ErrorMessage}", ex);
            }
             catch (JsonException jsonEx)
            {
                 _logger.LogError(jsonEx, "Failed to deserialize ffprobe JSON output for file: {FilePath}", filePath);
                 throw new InvalidOperationException("Failed to parse FFprobe JSON output.", jsonEx);
            }
            catch (OperationCanceledException)
            {
                 _logger.LogInformation("Media probing cancelled for file: {FilePath}", filePath);
                throw;
            }
            catch (Exception ex) // Catch any other unexpected exceptions
            {
                _logger.LogError(ex, "An unexpected error occurred during media probing for file: {FilePath}", filePath);
                throw new InvalidOperationException($"An unexpected error occurred while probing media file: {ex.Message}", ex);
            }
        }

        // ----- Helper Methods Moved from FFmpegService ----- 

        private MediaInfo ConvertToMediaInfo(FFprobeOutput probeData, string filePath)
        {
            var mediaInfo = new MediaInfo
            {
                FilePath = filePath,
                Format = new MediaFormatInfo
                {
                    FormatName = probeData.Format?.FormatName ?? string.Empty,
                    FormatLongName = probeData.Format?.FormatLongName ?? string.Empty,
                    Duration = double.TryParse(probeData.Format?.Duration, out var duration) ? duration : 0,
                    Size = probeData.Format?.Size ?? string.Empty,
                    BitRate = probeData.Format?.BitRate ?? string.Empty,
                    Tags = probeData.Format?.Tags ?? new Dictionary<string, string>()
                },
                HasAudio = false,  // Default to false, will be set to true if audio streams are found
                HasVideo = false   // Default to false, will be set to true if video streams are found
            };

            if (probeData.Streams == null) return mediaInfo;

            // Process streams
            foreach (var stream in probeData.Streams)
            {
                if (stream == null) continue;

                switch (stream.CodecType?.ToLowerInvariant())
                {
                    case "video":
                        var videoStream = new VideoStreamInfo
                        {
                            Index = stream.Index,
                            Codec = stream.CodecName ?? string.Empty,
                            CodecLongName = stream.CodecLongName ?? string.Empty,
                            Language = stream.Tags?.GetValueOrDefault("language"),
                            Width = stream.Width,
                            Height = stream.Height,
                            PixelFormat = stream.PixelFormat ?? string.Empty,
                            DisplayAspectRatio = stream.DisplayAspectRatio ?? string.Empty,
                            FrameRate = 0, // Default, calculated below
                            Bitrate = 0 // Default, calculated below
                        };

                        // Parse frame rate (e.g., "24000/1001")
                        if (!string.IsNullOrEmpty(stream.FrameRate))
                        {
                            string[] parts = stream.FrameRate.Split('/');
                            if (parts.Length == 2 && double.TryParse(parts[0], out double num) && double.TryParse(parts[1], out double den) && den != 0)
                            {
                                videoStream.FrameRate = num / den;
                            }
                        }

                        // Parse bit rate
                        if (!string.IsNullOrEmpty(stream.BitRate) && long.TryParse(stream.BitRate, out long parsedBitrate))
                        {
                            videoStream.Bitrate = parsedBitrate;
                        }

                        // Parse frame count
                        if (!string.IsNullOrEmpty(stream.NbFrames) && long.TryParse(stream.NbFrames, out long frameCount))
                        {
                            videoStream.FrameCount = frameCount;
                        }
                        
                        videoStream.Tags = stream.Tags ?? new Dictionary<string, string>();
                        mediaInfo.VideoStreams.Add(videoStream);
                        mediaInfo.HasVideo = true;
                        break;

                    case "audio":
                        var audioStream = new AudioStreamInfo
                        {
                            Index = stream.Index,
                            Codec = stream.CodecName ?? string.Empty,
                            CodecLongName = stream.CodecLongName ?? string.Empty,
                            Language = stream.Tags?.GetValueOrDefault("language"),
                            SampleRate = int.TryParse(stream.SampleRate, out var sr) ? sr : 0,
                            Channels = stream.Channels,
                            ChannelLayout = stream.ChannelLayout ?? string.Empty,
                            SampleFormat = stream.SampleFormat ?? string.Empty,
                            Bitrate = 0 // Default, calculated below
                        };

                        // Parse bit rate
                        if (!string.IsNullOrEmpty(stream.BitRate) && long.TryParse(stream.BitRate, out long audioBitrate))
                        {
                            audioStream.Bitrate = audioBitrate;
                        }
                        
                        audioStream.Tags = stream.Tags ?? new Dictionary<string, string>();
                        mediaInfo.AudioStreams.Add(audioStream);
                        mediaInfo.HasAudio = true;
                        break;

                    case "subtitle":
                        var subtitleStream = new SubtitleStreamInfo
                        {
                            Index = stream.Index,
                            Codec = stream.CodecName ?? string.Empty,
                            CodecLongName = stream.CodecLongName ?? string.Empty,
                            Language = stream.Tags?.GetValueOrDefault("language"),
                            SubtitleType = stream.CodecName ?? string.Empty, // Often the codec name indicates type (e.g., 'srt', 'ass')
                            Tags = stream.Tags ?? new Dictionary<string, string>()
                        };
                        mediaInfo.SubtitleStreams.Add(subtitleStream);
                        break;
                }
            }

            return mediaInfo;
        }

        // ----- FFprobe JSON Classes Moved from FFmpegService -----

        // Adding System.Text.Json.Serialization attributes for robustness
        private class FFprobeOutput
        {
            [System.Text.Json.Serialization.JsonPropertyName("streams")]
            public List<FFprobeStream>? Streams { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("format")]
            public FFprobeFormat? Format { get; set; }
        }

        private class FFprobeStream
        {
            [System.Text.Json.Serialization.JsonPropertyName("index")]
            public int Index { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("codec_name")]
            public string? CodecName { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("codec_long_name")]
            public string? CodecLongName { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("codec_type")]
            public string? CodecType { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("pix_fmt")]
            public string? PixelFormat { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("width")]
            public int Width { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("height")]
            public int Height { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("display_aspect_ratio")]
            public string? DisplayAspectRatio { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("sample_rate")]
            public string? SampleRate { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("channels")]
            public int Channels { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("channel_layout")]
            public string? ChannelLayout { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("sample_fmt")]
            public string? SampleFormat { get; set; }

            // Note: Sometimes ffprobe uses 'bit_rate' and sometimes 'BIT_RATE'
            [System.Text.Json.Serialization.JsonPropertyName("bit_rate")]
            public string? BitRate { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("r_frame_rate")] // Use r_frame_rate for reliable average frame rate
            public string? FrameRate { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("nb_frames")]
            public string? NbFrames { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("tags")]
            public Dictionary<string, string>? Tags { get; set; }
        }

        private class FFprobeFormat
        {
            [System.Text.Json.Serialization.JsonPropertyName("format_name")]
            public string? FormatName { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("format_long_name")]
            public string? FormatLongName { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("duration")]
            public string? Duration { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("size")]
            public string? Size { get; set; }

            // Note: Sometimes ffprobe uses 'bit_rate' and sometimes 'BIT_RATE'
            [System.Text.Json.Serialization.JsonPropertyName("bit_rate")]
            public string? BitRate { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("tags")]
            public Dictionary<string, string>? Tags { get; set; }
        }
    }
} 
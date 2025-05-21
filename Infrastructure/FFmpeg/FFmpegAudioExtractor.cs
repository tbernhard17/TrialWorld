using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Exceptions;
using TrialWorld.Infrastructure.Models.FFmpeg;

namespace TrialWorld.Infrastructure.FFmpeg
{
    /// <summary>
    /// Implementation of <see cref="IAudioExtractorService"/> using FFmpeg.
    /// </summary>
    public class FFmpegAudioExtractor : IAudioExtractorService
    {
        private readonly ILogger<FFmpegAudioExtractor> _logger;
        private readonly IProcessRunner _processRunner;
        private readonly IMediaInfoService _mediaInfoService;
        private readonly string _ffmpegPath;

        public FFmpegAudioExtractor(
            ILogger<FFmpegAudioExtractor> logger,
            IProcessRunner processRunner,
            IMediaInfoService mediaInfoService,
            IOptions<FFmpegOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
            _mediaInfoService = mediaInfoService ?? throw new ArgumentNullException(nameof(mediaInfoService));

            if (options?.Value == null) throw new ArgumentNullException(nameof(options));
            _ffmpegPath = options.Value.FFmpegPath ?? "ffmpeg";

            if (string.IsNullOrEmpty(_ffmpegPath))
            {
                 _logger.LogCritical("FFmpeg path is not configured.");
                 throw new ConfigurationValidationException("The path to FFmpeg is not configured in FFmpegOptions.");
            }
        }

        /// <inheritdoc />
        public async Task<string> ExtractAudioAsync(
            string mediaPath,
            string outputPath,
            string format = "mp3",
            int bitrate = 192,
            CancellationToken cancellationToken = default)
        {
            // Validate parameters
            if (string.IsNullOrEmpty(mediaPath))
                throw new ArgumentNullException(nameof(mediaPath));
            if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentNullException(nameof(outputPath));
            if (string.IsNullOrEmpty(format))
                throw new ArgumentNullException(nameof(format));
            if (bitrate <= 0)
                throw new ArgumentOutOfRangeException(nameof(bitrate), "Bitrate must be greater than zero.");

            // Ensure input file exists
            if (!File.Exists(mediaPath))
            {
                _logger.LogError("Audio extraction source file not found: {MediaPath}", mediaPath);
                throw new FileNotFoundException("The specified media file does not exist.", mediaPath);
            }
            
            // Ensure the output directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

            _logger.LogInformation("Starting audio extraction: Source={Source}, Target={Target}, Format={Format}, Bitrate={Bitrate}k", 
                mediaPath, outputPath, format, bitrate);

            // Validate that the input file has audio using the dedicated service
            var mediaInfo = await _mediaInfoService.GetMediaInfoAsync(mediaPath, cancellationToken);
            if (!mediaInfo.HasAudio)
            {
                _logger.LogWarning("Attempted to extract audio from file with no audio stream: {MediaPath}", mediaPath);
                throw new InvalidOperationException("The specified file does not contain audio content.");
            }

            // Build FFmpeg arguments
            string audioCodec = GetAudioCodec(format);
            string arguments = $"-i \"{mediaPath}\" -vn -c:a {audioCodec} -b:a {bitrate}k \"{outputPath}\" -y";

            try
            {
                // Run FFmpeg process using the process runner
                await _processRunner.RunProcessAsync(_ffmpegPath, arguments, cancellationToken);
                _logger.LogInformation("Audio extraction completed successfully: {OutputPath}", outputPath);
                return outputPath;
            }
            catch (ExternalProcessException ex)
            {
                 _logger.LogError(ex, "FFmpeg process failed during audio extraction. ExitCode={ExitCode}, Error={Error}", ex.ExitCode, ex.ErrorMessage);
                 throw new InvalidOperationException($"Failed to extract audio: FFmpeg process failed (ExitCode: {ex.ExitCode}). Error: {ex.ErrorMessage}", ex);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Audio extraction cancelled for source: {MediaPath}", mediaPath);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during audio extraction for source: {MediaPath}", mediaPath);
                throw new InvalidOperationException($"An unexpected error occurred during audio extraction: {ex.Message}", ex); 
            }
        }

        private string GetAudioCodec(string format)
        {
            switch (format.ToLowerInvariant())
            {
                case "mp3":
                    return "libmp3lame";
                case "aac":
                    return "aac"; // Consider specifying a quality profile if needed (e.g., -q:a 2)
                case "opus":
                    return "libopus";
                case "flac":
                    return "flac";
                case "wav":
                    return "pcm_s16le";
                default:
                    _logger.LogWarning("Unknown audio format '{Format}' requested for extraction. Using format name as codec.", format);
                    return format; // Use the format directly as codec, might fail
            }
        }
    }
} 
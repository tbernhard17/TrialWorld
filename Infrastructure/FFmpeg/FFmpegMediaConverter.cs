using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Exceptions;
using TrialWorld.Core.Models.Analysis;
using TrialWorld.Infrastructure.Models.FFmpeg;

namespace TrialWorld.Infrastructure.FFmpeg
{
    /// <summary>
    /// Implementation of <see cref="IMediaConversionService"/> using FFmpeg.
    /// </summary>
    public class FFmpegMediaConverter : IMediaConversionService
    {
        private readonly ILogger<FFmpegMediaConverter> _logger;
        private readonly IProcessRunner _processRunner;
        private readonly IMediaInfoService _mediaInfoService;
        private readonly string _ffmpegPath;

        public FFmpegMediaConverter(
            ILogger<FFmpegMediaConverter> logger,
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
        public async Task<string> ConvertMediaAsync(
            string inputPath,
            string outputPath,
            MediaConversionOptions options,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            // Validate parameters
            if (string.IsNullOrEmpty(inputPath))
                throw new ArgumentNullException(nameof(inputPath));
             if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentNullException(nameof(outputPath));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            // Ensure input file exists
            if (!File.Exists(inputPath))
            {
                 _logger.LogError("Media conversion source file not found: {InputPath}", inputPath);
                throw new FileNotFoundException("The specified input file does not exist.", inputPath);
            }

            // Ensure the output directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

             _logger.LogInformation("Starting media conversion: Source={Source}, Target={Target}, Options={Options}", 
                 inputPath, outputPath, options); // Consider logging options details selectively

            // Get media information to determine existing streams
            var mediaInfo = await _mediaInfoService.GetMediaInfoAsync(inputPath, cancellationToken);

            // Build FFmpeg arguments
            var argumentsBuilder = new List<string>();
            argumentsBuilder.Add($"-i \"{inputPath}\"");

            // Video options (only if video stream exists)
            if (mediaInfo.HasVideo)
            {
                if (string.IsNullOrEmpty(options.VideoCodec))
                {
                    argumentsBuilder.Add("-c:v copy");
                }
                else
                {
                    argumentsBuilder.Add($"-c:v {options.VideoCodec}");
                    if (options.VideoBitrate > 0)
                    {
                        argumentsBuilder.Add($"-b:v {options.VideoBitrate}k");
                    }
                }
            } else {
                 argumentsBuilder.Add("-vn"); // Explicitly disable video if no source video
            }

            // Audio options (only if audio stream exists)
            if (mediaInfo.HasAudio)
            {
                if (string.IsNullOrEmpty(options.AudioCodec))
                {
                    argumentsBuilder.Add("-c:a copy");
                }
                else
                {
                    argumentsBuilder.Add($"-c:a {options.AudioCodec}");
                    if (options.AudioBitrate > 0)
                    {
                        argumentsBuilder.Add($"-b:a {options.AudioBitrate}k");
                    }
                }
            } else {
                argumentsBuilder.Add("-an"); // Explicitly disable audio if no source audio
            }

            // Resolution (apply only if video is being processed)
            if (mediaInfo.HasVideo && (options.Width.HasValue || options.Height.HasValue))
            {
                int width = options.Width ?? -1; // -1 tells FFmpeg to preserve aspect ratio
                int height = options.Height ?? -1;
                argumentsBuilder.Add($"-vf \"scale={width}:{height}\"");
            }

            // Frame rate (apply only if video is being processed)
            if (mediaInfo.HasVideo && options.FrameRate > 0)
            {
                argumentsBuilder.Add($"-r {options.FrameRate}");
            }

            // Output path and overwrite flag
            argumentsBuilder.Add($"\"{outputPath}\" -y");

            var arguments = string.Join(" ", argumentsBuilder);
            _logger.LogDebug("Generated FFmpeg arguments for conversion: {Arguments}", arguments);

            try
            {
                if (progress != null)
                {
                    // Create progress handler only if needed
                    double totalDuration = (mediaInfo?.Format?.Duration) ?? 0.0;
                    var progressHandler = CreateProgressHandler(progress, totalDuration);
                    await _processRunner.RunProcessWithProgressAsync(_ffmpegPath, arguments, progressHandler, cancellationToken);
                }
                else
                {
                    // Run without progress reporting
                    await _processRunner.RunProcessAsync(_ffmpegPath, arguments, cancellationToken);
                }
                
                _logger.LogInformation("Media conversion completed successfully: {OutputPath}", outputPath);
                return outputPath;
            }
            catch (ExternalProcessException ex)
            {
                _logger.LogError(ex, "FFmpeg process failed during media conversion. ExitCode={ExitCode}, Error={Error}", ex.ExitCode, ex.ErrorMessage);
                throw new InvalidOperationException($"Failed to convert media: FFmpeg process failed (ExitCode: {ex.ExitCode}). Error: {ex.ErrorMessage}", ex);
            }
             catch (OperationCanceledException)
            {
                 _logger.LogInformation("Media conversion cancelled for source: {InputPath}", inputPath);
                throw;
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An unexpected error occurred during media conversion for source: {InputPath}", inputPath);
                 throw new InvalidOperationException($"An unexpected error occurred during media conversion: {ex.Message}", ex); 
            }
        }

        private IProgress<string> CreateProgressHandler(IProgress<double> progressReporter, double totalDuration)
        {
            return new Progress<string>(outputLine =>
            {
                if (totalDuration <= 0) return; // Cannot calculate progress without duration

                // Parse time=HH:MM:SS.ms from FFmpeg stderr
                if (outputLine.Contains("time="))
                {
                    try
                    {
                        var timeMatch = Regex.Match(outputLine, @"time=(\d{2}):(\d{2}):(\d{2})\.(\d{2})");
                        if (timeMatch.Success)
                        {
                            double hours = double.Parse(timeMatch.Groups[1].Value);
                            double minutes = double.Parse(timeMatch.Groups[2].Value);
                            double seconds = double.Parse(timeMatch.Groups[3].Value);
                            double milliseconds = double.Parse(timeMatch.Groups[4].Value);
                            double currentTime = (hours * 3600) + (minutes * 60) + seconds + (milliseconds / 100.0);

                            double progressValue = Math.Clamp(currentTime / totalDuration, 0.0, 1.0);
                            progressReporter.Report(progressValue); 
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error parsing FFmpeg progress line: {OutputLine}", outputLine);
                    }
                }
            });
        }
    }
} 
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
using TrialWorld.Core.Services;

namespace TrialWorld.Infrastructure.FFmpeg
{
    /// <summary>
    /// Implementation of thumbnail extraction using FFmpeg.
    /// </summary>
    public class FFmpegThumbnailExtractor : IThumbnailExtractor
    {
        private readonly IProcessRunner _processRunner;
        private readonly IMediaInfoService _mediaInfoService;
        private readonly FFmpegOptions _options;
        private readonly string _ffmpegPath;
        private readonly ILogger<FFmpegThumbnailExtractor> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FFmpegThumbnailExtractor"/> class.
        /// </summary>
        /// <param name="processRunner">Process runner utility.</param>
        /// <param name="mediaInfoService">The media info service.</param>
        /// <param name="options">FFmpeg options.</param>
        /// <param name="logger">Logger.</param>
        public FFmpegThumbnailExtractor(
            IProcessRunner processRunner,
            IMediaInfoService mediaInfoService,
            IOptions<FFmpegOptions> options,
            ILogger<FFmpegThumbnailExtractor> logger)
        {
            _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
            _mediaInfoService = mediaInfoService ?? throw new ArgumentNullException(nameof(mediaInfoService));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _ffmpegPath = _options.FFmpegPath ?? "ffmpeg";
            if (!File.Exists(_ffmpegPath))
            {
                _logger.LogCritical("FFmpeg binary not found at path configured for FFmpegThumbnailExtractor: {Path}", _ffmpegPath);
                throw new FileNotFoundException($"FFmpeg binary not found at path: {_ffmpegPath}", _ffmpegPath);
            }

            var thumbDir = _options.ThumbnailDirectory;
            if (!string.IsNullOrEmpty(thumbDir))
            {
                try { Directory.CreateDirectory(thumbDir); }
                catch (Exception ex) { _logger.LogError(ex, "Failed to create base thumbnail directory: {Directory}", thumbDir); }
            }
            else
            {
                _logger.LogWarning("ThumbnailDirectory is not configured in FFmpegOptions.");
            }
        }

        /// <inheritdoc />
        public async Task ExtractThumbnailAsync(
            string mediaPath,
            string outputPath,
            TimeSpan position = default,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(mediaPath))
                throw new ArgumentException("Media path cannot be empty", nameof(mediaPath));

            if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentException("Output path cannot be empty", nameof(outputPath));

            if (!File.Exists(mediaPath))
                throw new FileNotFoundException("Media file not found", mediaPath);

            var outputDir = Path.GetDirectoryName(outputPath);
            if(!string.IsNullOrEmpty(outputDir))
            {
                try { Directory.CreateDirectory(outputDir); }
                catch (Exception ex) { throw new IOException($"Failed to create output directory '{outputDir}': {ex.Message}", ex); }
            }

            if (position == default)
            {
                position = TimeSpan.FromSeconds(5);
            }

            try
            {
                var arguments = $"-hide_banner -loglevel error -y -ss {position.TotalSeconds} -i \"{mediaPath}\" -vframes 1 -q:v 2 \"{outputPath}\"";

                _logger.LogInformation("Extracting thumbnail at {Timestamp}s from {MediaPath} to {ThumbnailPath}", position.TotalSeconds, mediaPath, outputPath);

                await _processRunner.RunProcessAsync(_ffmpegPath, arguments, cancellationToken);

                _logger.LogInformation("Successfully extracted thumbnail to {OutputPath}", outputPath);
            }
            catch (ExternalProcessException ex)
            {
                _logger.LogError(ex, "Failed to extract thumbnail. FFmpeg process failed. ExitCode={ExitCode}, Error={Error}", ex.ExitCode, ex.ErrorMessage);
                throw new FFmpegProcessingException($"Failed to extract thumbnail. FFmpeg exited with code {ex.ExitCode}: {ex.ErrorMessage}", ex);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Thumbnail extraction cancelled for {MediaPath}", mediaPath);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Thumbnail extraction failed unexpectedly for {MediaPath}", mediaPath);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<string[]> ExtractMultipleThumbnailsAsync(
            string mediaPath,
            string outputDirectory,
            int count,
            string fileNameTemplate = "thumb_{0:D4}.jpg",
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(mediaPath))
                throw new ArgumentException("Media path cannot be empty", nameof(mediaPath));

            if (string.IsNullOrEmpty(outputDirectory))
                throw new ArgumentException("Output directory cannot be empty", nameof(outputDirectory));

            if (!File.Exists(mediaPath))
                throw new FileNotFoundException("Media file not found", mediaPath);

            if (count <= 0)
                throw new ArgumentException("Count must be greater than zero", nameof(count));

            Directory.CreateDirectory(outputDirectory);

            try
            {
                var mediaInfo = await _mediaInfoService.GetMediaInfoAsync(mediaPath, cancellationToken);
                if (mediaInfo == null || mediaInfo.Duration == TimeSpan.Zero)
                {
                    _logger.LogWarning("Could not retrieve valid media duration for {MediaPath}. Cannot extract multiple thumbnails.", mediaPath);
                    return Array.Empty<string>();
                }

                var duration = mediaInfo.Duration;
                var interval = duration.TotalSeconds / (count + 1);
                var outputPaths = new string[count];

                _logger.LogInformation("Extracting {Count} thumbnails from {MediaPath} with duration {Duration}",
                    count, mediaPath, duration);

                for (int i = 0; i < count; i++)
                {
                    var position = TimeSpan.FromSeconds((i + 1) * interval);
                    var outputPath = Path.Combine(outputDirectory, string.Format(fileNameTemplate, i + 1));

                    await ExtractThumbnailAsync(mediaPath, outputPath, position, cancellationToken);

                    outputPaths[i] = outputPath;
                }

                return outputPaths;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting multiple thumbnails from {MediaPath}", mediaPath);
                throw;
            }
        }
    }
}
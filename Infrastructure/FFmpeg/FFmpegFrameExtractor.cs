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
    /// Implementation of <see cref="IFrameExtractorService"/> using FFmpeg.
    /// </summary>
    public class FFmpegFrameExtractor : IFrameExtractorService
    {
        private readonly ILogger<FFmpegFrameExtractor> _logger;
        private readonly IProcessRunner _processRunner;
        private readonly string _ffmpegPath;

        public FFmpegFrameExtractor(
            ILogger<FFmpegFrameExtractor> logger,
            IProcessRunner processRunner,
            IOptions<FFmpegOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
            
            if (options?.Value == null) throw new ArgumentNullException(nameof(options));
            _ffmpegPath = options.Value.FFmpegPath ?? "ffmpeg";

            if (string.IsNullOrEmpty(_ffmpegPath))
            {
                _logger.LogCritical("FFmpeg path is not configured.");
                throw new ConfigurationValidationException("The path to FFmpeg is not configured in FFmpegOptions.");
            }
        }

        /// <inheritdoc />
        public async Task<string> ExtractFrameAsync(
            string videoPath, 
            string outputPath, 
            double timestamp, 
            CancellationToken cancellationToken = default)
        {
             // Validate parameters
            if (string.IsNullOrEmpty(videoPath))
                throw new ArgumentNullException(nameof(videoPath));
            if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentNullException(nameof(outputPath));
            if (timestamp < 0)
                throw new ArgumentOutOfRangeException(nameof(timestamp), "Timestamp must be non-negative.");

            // Ensure input file exists
            if (!File.Exists(videoPath))
            {
                _logger.LogError("Frame extraction source video not found: {VideoPath}", videoPath);
                throw new FileNotFoundException("The specified video file does not exist.", videoPath);
            }

            // Ensure the output directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

             _logger.LogInformation("Extracting frame: Source={Source}, Target={Target}, Timestamp={Timestamp}s", 
                 videoPath, outputPath, timestamp);

            // Build FFmpeg arguments
            // -ss before -i seeks faster but less accurately. Use after -i for precision if needed.
            // -frames:v 1 extracts only one frame.
            // -q:v 2 is generally high quality for JPG/PNG.
            string timestampArg = timestamp.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string arguments = $"-ss {timestampArg} -i \"{videoPath}\" -frames:v 1 -q:v 2 \"{outputPath}\" -y";
            _logger.LogDebug("Running FFmpeg for frame extraction: {Arguments}", arguments);

            try
            {
                // Run FFmpeg process
                await _processRunner.RunProcessAsync(_ffmpegPath, arguments, cancellationToken);
                 _logger.LogInformation("Frame extraction completed successfully: {OutputPath}", outputPath);
                return outputPath;
            }
            catch (ExternalProcessException ex)
            {
                 _logger.LogError(ex, "FFmpeg process failed during frame extraction. ExitCode={ExitCode}, Error={Error}", ex.ExitCode, ex.ErrorMessage);
                 throw new InvalidOperationException($"Failed to extract frame: FFmpeg process failed (ExitCode: {ex.ExitCode}). Error: {ex.ErrorMessage}", ex);
            }
             catch (OperationCanceledException)
            {
                 _logger.LogInformation("Frame extraction cancelled for source: {VideoPath}", videoPath);
                throw;
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An unexpected error occurred during frame extraction for source: {VideoPath}", videoPath);
                 throw new InvalidOperationException($"An unexpected error occurred during frame extraction: {ex.Message}", ex); 
            }
        }
    }
} 
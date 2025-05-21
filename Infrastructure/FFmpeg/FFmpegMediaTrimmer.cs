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
    /// Implementation of <see cref="IMediaTrimmerService"/> using FFmpeg.
    /// </summary>
    public class FFmpegMediaTrimmer : IMediaTrimmerService
    {
        private readonly ILogger<FFmpegMediaTrimmer> _logger;
        private readonly IProcessRunner _processRunner;
        private readonly string _ffmpegPath;

        public FFmpegMediaTrimmer(
            ILogger<FFmpegMediaTrimmer> logger,
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
        public async Task<string> TrimMediaAsync(
            string inputPath,
            string outputPath,
            double startTime,
            double? endTime = null,
            bool preserveQuality = true,
            CancellationToken cancellationToken = default)
        {
            // Validate parameters
            if (string.IsNullOrEmpty(inputPath))
                throw new ArgumentNullException(nameof(inputPath));
            if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentNullException(nameof(outputPath));
            if (startTime < 0)
                throw new ArgumentOutOfRangeException(nameof(startTime), "Start time must be non-negative.");
            if (endTime.HasValue && endTime.Value <= startTime)
                throw new ArgumentOutOfRangeException(nameof(endTime), "End time must be greater than start time.");

            // Ensure input file exists
            if (!File.Exists(inputPath))
            {
                _logger.LogError("Media trim source file not found: {InputPath}", inputPath);
                throw new FileNotFoundException("The specified input file does not exist.", inputPath);
            }

            // Ensure the output directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

             _logger.LogInformation("Starting media trim: Source={Source}, Target={Target}, Start={Start}, End={End}, PreserveQuality={Preserve}", 
                 inputPath, outputPath, startTime, endTime?.ToString() ?? "EOF", preserveQuality);

            // Build FFmpeg arguments
            // Use CultureInfo.InvariantCulture for reliable double to string conversion
            string startTimeArg = startTime.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var durationArg = endTime.HasValue ? $"-t {(endTime.Value - startTime).ToString(System.Globalization.CultureInfo.InvariantCulture)}" : string.Empty;

            string arguments;
            if (preserveQuality)
            {
                // Stream copy mode for fast trimming without re-encoding (less accurate seeking)
                arguments = $"-ss {startTimeArg} -i \"{inputPath}\" {durationArg} -c copy -map 0 \"{outputPath}\" -y";
                _logger.LogDebug("Using stream copy mode for trimming.");
            }
            else
            {
                // Re-encode mode for accurate trimming (slower but more accurate seeking)
                 // Note: Input seeking (-ss before -i) is faster but less accurate. Output seeking (-ss after -i) is slower but precise.
                 // Choosing precise output seeking here as preserveQuality is false.
                arguments = $"-i \"{inputPath}\" -ss {startTimeArg} {durationArg} -map 0 \"{outputPath}\" -y";
                 _logger.LogDebug("Using re-encode mode for trimming (precise seeking).");
            }

            try
            {
                // Run FFmpeg process using the process runner
                await _processRunner.RunProcessAsync(_ffmpegPath, arguments, cancellationToken);
                 _logger.LogInformation("Media trim completed successfully: {OutputPath}", outputPath);
                return outputPath;
            }
            catch (ExternalProcessException ex)
            {
                 _logger.LogError(ex, "FFmpeg process failed during media trim. ExitCode={ExitCode}, Error={Error}", ex.ExitCode, ex.ErrorMessage);
                 throw new InvalidOperationException($"Failed to trim media: FFmpeg process failed (ExitCode: {ex.ExitCode}). Error: {ex.ErrorMessage}", ex);
            }
             catch (OperationCanceledException)
            {
                 _logger.LogInformation("Media trim cancelled for source: {InputPath}", inputPath);
                throw;
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An unexpected error occurred during media trim for source: {InputPath}", inputPath);
                 throw new InvalidOperationException($"An unexpected error occurred during media trim: {ex.Message}", ex); 
            }
        }
    }
} 
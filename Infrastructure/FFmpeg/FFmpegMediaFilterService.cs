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
    /// Implementation of <see cref="IMediaFilterService"/> using FFmpeg.
    /// </summary>
    public class FFmpegMediaFilterService : IMediaFilterService
    {
        private readonly ILogger<FFmpegMediaFilterService> _logger;
        private readonly IProcessRunner _processRunner;
        private readonly IMediaInfoService _mediaInfoService; // Needed to check stream existence
        private readonly string _ffmpegPath;

        public FFmpegMediaFilterService(
            ILogger<FFmpegMediaFilterService> logger,
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
        public async Task<string> ApplyFilterAsync(
            string inputPath,
            string outputPath,
            FilterStreamType filterStreamType,
            string filterChain,
            CancellationToken cancellationToken = default)
        {
            // Validate parameters
            if (string.IsNullOrEmpty(inputPath))
                throw new ArgumentNullException(nameof(inputPath));
            if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentNullException(nameof(outputPath));
            if (string.IsNullOrEmpty(filterChain))
                throw new ArgumentNullException(nameof(filterChain));

            // Ensure input file exists
            if (!File.Exists(inputPath))
            {
                _logger.LogError("Media filter source file not found: {InputPath}", inputPath);
                throw new FileNotFoundException("The specified input file does not exist.", inputPath);
            }

            // Ensure the output directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

            _logger.LogInformation("Applying {StreamType} filter: Source={Source}, Target={Target}, Filter='{Filter}'",
                filterStreamType, inputPath, outputPath, filterChain);

            // Verify the file has the necessary stream type
            var mediaInfo = await _mediaInfoService.GetMediaInfoAsync(inputPath, cancellationToken);
            string filterArgPrefix;
            string copyOtherStreamArg;

            switch (filterStreamType)
            {
                case FilterStreamType.Audio:
                    if (!mediaInfo.HasAudio)
                    {
                        _logger.LogWarning("Attempted to apply audio filter to file with no audio stream: {InputPath}", inputPath);
                        throw new InvalidOperationException("The specified file does not contain an audio stream to filter.");
                    }
                    filterArgPrefix = "-af";
                    copyOtherStreamArg = mediaInfo.HasVideo ? "-c:v copy" : string.Empty; // Copy video if it exists
                    break;
                case FilterStreamType.Video:
                    if (!mediaInfo.HasVideo)
                    {
                        _logger.LogWarning("Attempted to apply video filter to file with no video stream: {InputPath}", inputPath);
                        throw new InvalidOperationException("The specified file does not contain a video stream to filter.");
                    }
                    filterArgPrefix = "-vf";
                    copyOtherStreamArg = mediaInfo.HasAudio ? "-c:a copy" : string.Empty; // Copy audio if it exists
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(filterStreamType));
            }

            // Build FFmpeg arguments
            // Using filter_complex might be more robust for complex chains, but vf/af is simpler for basic use.
            string arguments = $"-i \"{inputPath}\" {filterArgPrefix} \"{filterChain}\" {copyOtherStreamArg} \"{outputPath}\" -y";

            try
            {
                // Run FFmpeg process using the process runner
                await _processRunner.RunProcessAsync(_ffmpegPath, arguments, cancellationToken);
                _logger.LogInformation("Media filter application completed successfully: {OutputPath}", outputPath);
                return outputPath;
            }
            catch (ExternalProcessException ex)
            {
                _logger.LogError(ex, "FFmpeg process failed during media filter application. ExitCode={ExitCode}, Error={Error}", ex.ExitCode, ex.ErrorMessage);
                throw new InvalidOperationException($"Failed to apply media filter: FFmpeg process failed (ExitCode: {ex.ExitCode}). Error: {ex.ErrorMessage}", ex);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Media filter application cancelled for source: {InputPath}", inputPath);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during media filter application for source: {InputPath}", inputPath);
                throw new InvalidOperationException($"An unexpected error occurred during media filter application: {ex.Message}", ex);
            }
        }
    }
} 
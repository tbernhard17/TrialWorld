using System;
using System.IO;
using System.Text;
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
    /// Implementation of <see cref="IGifCreationService"/> using FFmpeg.
    /// Uses a two-pass approach (palette generation + palette use) for better quality.
    /// </summary>
    public class FFmpegGifCreator : IGifCreationService
    {
        private readonly ILogger<FFmpegGifCreator> _logger;
        private readonly IProcessRunner _processRunner;
        // private readonly IMediaInfoService _mediaInfoService; // Not strictly needed if we trust input video has video
        private readonly string _ffmpegPath;
        private readonly string _tempDirectory;

        public FFmpegGifCreator(
            ILogger<FFmpegGifCreator> logger,
            IProcessRunner processRunner,
            // IMediaInfoService mediaInfoService, 
            IOptions<FFmpegOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
            // _mediaInfoService = mediaInfoService ?? throw new ArgumentNullException(nameof(mediaInfoService));

            if (options?.Value == null) throw new ArgumentNullException(nameof(options));
            _ffmpegPath = options.Value.FFmpegPath ?? "ffmpeg";
             _tempDirectory = options.Value.TempDirectory ?? Path.Combine(Path.GetTempPath(), "FFmpegTemp");

            if (string.IsNullOrEmpty(_ffmpegPath))
            {
                _logger.LogCritical("FFmpeg path is not configured.");
                throw new ConfigurationValidationException("The path to FFmpeg is not configured in FFmpegOptions.");
            }
             Directory.CreateDirectory(_tempDirectory);
        }

        /// <inheritdoc />
        public async Task<string> CreateGifFromVideoAsync(
            string videoPath,
            string outputPath,
            TimeSpan startTime,
            TimeSpan? duration = null,
            int frameRate = 10,
            int width = 320,
            int quality = 85,
            bool dither = true,
            CancellationToken cancellationToken = default)
        {
            // Validate parameters
            ArgumentNullException.ThrowIfNull(videoPath, nameof(videoPath));
            ArgumentNullException.ThrowIfNull(outputPath, nameof(outputPath));

            if (frameRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(frameRate), "Frame rate must be a positive value.");
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be a positive value.");
            if (quality is < 1 or > 100) // Quality maps to palette colors (max 255)
                throw new ArgumentOutOfRangeException(nameof(quality), "Quality must be between 1 and 100.");

            // Ensure input file exists
            if (!File.Exists(videoPath))
            {
                _logger.LogError("GIF creation source video not found: {VideoPath}", videoPath);
                throw new FileNotFoundException("The video file does not exist.", videoPath);
            }

            // Ensure the output directory exists
            string? outputDirectory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
             _logger.LogInformation("Creating GIF: Source={Source}, Target={Target}, Start={Start}, Duration={Dur}, FPS={Fps}, Width={W}", 
                 videoPath, outputPath, startTime, duration?.ToString() ?? "End", frameRate, width);

            // Create a temporary file path for the color palette
            string tempPalettePath = Path.Combine(
                _tempDirectory,
                $"palette_{Path.GetFileNameWithoutExtension(outputPath)}_{Guid.NewGuid()}.png");
             _logger.LogDebug("Temporary palette path: {PalettePath}", tempPalettePath);

            try
            {
                // --- Pass 1: Generate Color Palette --- 
                 _logger.LogDebug("Starting GIF Pass 1: Palette Generation");
                string startTimeArgs = $"-ss {startTime.TotalSeconds}";
                string durationArgs = duration.HasValue ? $"-t {duration.Value.TotalSeconds}" : string.Empty;
                // Calculate max colors for palette based on quality (linear scale 1-100 -> ~2-255 colors)
                int maxColors = Math.Clamp((int)Math.Round(quality * 2.53 + 2), 2, 255); 
                string paletteFilters = $"fps={frameRate},scale={width}:-1:flags=lanczos";

                var paletteArgsBuilder = new StringBuilder();
                paletteArgsBuilder.Append($"{startTimeArgs} {durationArgs} -i \"{videoPath}\" ");
                paletteArgsBuilder.Append($"-vf \"{paletteFilters},palettegen=stats_mode=full:max_colors={maxColors}:reserve_transparent=0\" ");
                paletteArgsBuilder.Append($"-y \"{tempPalettePath}\"");
                string paletteArgs = paletteArgsBuilder.ToString();
                 _logger.LogDebug("Running FFmpeg for palette generation: {Arguments}", paletteArgs);
                await _processRunner.RunProcessAsync(_ffmpegPath, paletteArgs, cancellationToken);
                _logger.LogDebug("Palette generation completed.");

                // --- Pass 2: Generate GIF using Palette --- 
                 _logger.LogDebug("Starting GIF Pass 2: GIF Creation using Palette");
                int ditherValue = dither ? 1 : 0; // Map bool to 0 or 1 for bayer_scale
                 // Use paletteuse filter with specified dithering
                string gifFilters = $"{paletteFilters},paletteuse=dither=bayer:bayer_scale={ditherValue}";

                var gifArgsBuilder = new StringBuilder();
                gifArgsBuilder.Append($"{startTimeArgs} {durationArgs} -i \"{videoPath}\" -i \"{tempPalettePath}\" ");
                gifArgsBuilder.Append($"-lavfi \"{gifFilters}\" -y \"{outputPath}\"");
                string gifArgs = gifArgsBuilder.ToString();
                 _logger.LogDebug("Running FFmpeg for GIF creation: {Arguments}", gifArgs);
                await _processRunner.RunProcessAsync(_ffmpegPath, gifArgs, cancellationToken);
                 _logger.LogInformation("GIF creation completed successfully: {OutputPath}", outputPath);

                return outputPath;
            }
            catch (ExternalProcessException ex)
            {
                 _logger.LogError(ex, "FFmpeg process failed during GIF creation. ExitCode={ExitCode}, Error={Error}", ex.ExitCode, ex.ErrorMessage);
                 throw new InvalidOperationException($"Failed to create GIF: FFmpeg process failed (ExitCode: {ex.ExitCode}). Error: {ex.ErrorMessage}", ex);
            }
             catch (OperationCanceledException)
            {
                 _logger.LogInformation("GIF creation cancelled for source: {VideoPath}", videoPath);
                throw;
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An unexpected error occurred during GIF creation for source: {VideoPath}", videoPath);
                 throw new InvalidOperationException($"An unexpected error occurred during GIF creation: {ex.Message}", ex); 
            }
            finally
            {
                // Clean up the temporary palette file
                if (File.Exists(tempPalettePath))
                {
                    try 
                    { 
                        File.Delete(tempPalettePath);
                        _logger.LogDebug("Deleted temporary palette file: {PalettePath}", tempPalettePath);
                    }
                    catch (IOException ioEx) { _logger.LogWarning(ioEx, "Failed to delete temporary palette file: {PalettePath}", tempPalettePath); }
                }
            }
        }
    }
} 
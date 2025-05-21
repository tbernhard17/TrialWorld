using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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
    /// Implementation of <see cref="IWaveformService"/> using FFmpeg.
    /// </summary>
    public class FFmpegWaveformService : IWaveformService
    {
        private readonly ILogger<FFmpegWaveformService> _logger;
        private readonly IProcessRunner _processRunner;
        private readonly IMediaInfoService _mediaInfoService;
        private readonly string _ffmpegPath;
        private readonly string _tempDirectory; // Needed for waveform data generation

        public FFmpegWaveformService(
            ILogger<FFmpegWaveformService> logger,
            IProcessRunner processRunner,
            IMediaInfoService mediaInfoService,
            IOptions<FFmpegOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
            _mediaInfoService = mediaInfoService ?? throw new ArgumentNullException(nameof(mediaInfoService));

            if (options?.Value == null) throw new ArgumentNullException(nameof(options));
            _ffmpegPath = options.Value.FFmpegPath ?? "ffmpeg";
            _tempDirectory = options.Value.TempDirectory ?? Path.Combine(Path.GetTempPath(), "FFmpegTemp"); // Get temp dir from options

            if (string.IsNullOrEmpty(_ffmpegPath))
            {
                _logger.LogCritical("FFmpeg path is not configured.");
                throw new ConfigurationValidationException("The path to FFmpeg is not configured in FFmpegOptions.");
            }
            // Ensure temp directory exists (ProcessRunner doesn't create it)
            Directory.CreateDirectory(_tempDirectory);
        }

        /// <inheritdoc />
        public async Task<float[]> GenerateWaveformDataAsync(
            string audioPath,
            int sampleCount = 1000,
            CancellationToken cancellationToken = default)
        {
            // Validate parameters
            if (string.IsNullOrEmpty(audioPath))
                throw new ArgumentNullException(nameof(audioPath));
            if (sampleCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleCount), "Sample count must be greater than zero.");

            // Ensure input file exists
            if (!File.Exists(audioPath))
            {
                _logger.LogError("Waveform source file not found: {AudioPath}", audioPath);
                throw new FileNotFoundException("The specified audio file does not exist.", audioPath);
            }

             _logger.LogInformation("Generating waveform data: Source={Source}, SampleCount={Count}", audioPath, sampleCount);

            // Get media information
            var mediaInfo = await _mediaInfoService.GetMediaInfoAsync(audioPath, cancellationToken);
            if (!mediaInfo.HasAudio)
            {
                _logger.LogWarning("Attempted to generate waveform from file with no audio stream: {AudioPath}", audioPath);
                throw new InvalidOperationException("The specified file does not contain audio content.");
            }

            // Create temporary output file for raw audio samples
            string tempFilePath = Path.Combine(_tempDirectory, $"waveform_{Path.GetRandomFileName()}.dat");

            try
            {
                // Build FFmpeg arguments to extract audio as raw PCM samples (mono, 16-bit signed little-endian)
                // Resample to double the target sample count for better peak detection during downsampling
                int internalSampleRate = sampleCount * 2; 
                string arguments = $"-i \"{audioPath}\" -ac 1 -ar {internalSampleRate} -filter:a \"aresample={internalSampleRate}\" -map 0:a -c:a pcm_s16le -f s16le \"{tempFilePath}\" -y";
                _logger.LogDebug("Running FFmpeg for raw audio data: {Arguments}", arguments);

                // Run FFmpeg process
                await _processRunner.RunProcessAsync(_ffmpegPath, arguments, cancellationToken);

                // Read the raw PCM data file
                 _logger.LogDebug("Reading raw PCM data from temporary file: {TempFilePath}", tempFilePath);
                byte[] rawData = await File.ReadAllBytesAsync(tempFilePath, cancellationToken);

                // Convert raw 16-bit PCM samples to normalized float array (-1.0 to 1.0)
                float[] samples = new float[rawData.Length / 2]; // 2 bytes per sample
                for (int i = 0; i < samples.Length; i++)
                {
                    short pcmValue = BitConverter.ToInt16(rawData, i * 2);
                    samples[i] = pcmValue / 32768.0f; // Normalize by max value of Int16
                }
                _logger.LogDebug("Converted raw data to {SampleCount} float samples.", samples.Length);

                // Downsample to the requested sample count by taking absolute peaks
                if (samples.Length > sampleCount)
                {
                     _logger.LogDebug("Downsampling from {ActualCount} to {TargetCount} samples.", samples.Length, sampleCount);
                    float[] downsampled = new float[sampleCount];
                    int samplesPerValue = samples.Length / sampleCount;

                    for (int i = 0; i < sampleCount; i++)
                    {
                        float maxAbs = 0;
                        int startIdx = i * samplesPerValue;
                        int endIdx = Math.Min(startIdx + samplesPerValue, samples.Length);

                        for (int j = startIdx; j < endIdx; j++)
                        {
                            float absValue = Math.Abs(samples[j]);
                            if (absValue > maxAbs)
                                maxAbs = absValue;
                        }
                        downsampled[i] = maxAbs;
                    }
                     _logger.LogInformation("Successfully generated waveform data: Source={Source}, SampleCount={Count}", audioPath, sampleCount);
                    return downsampled;
                }
                else
                {
                    // If we got fewer samples than requested (maybe very short audio?), return what we have
                     _logger.LogWarning("Generated fewer samples ({ActualCount}) than requested ({TargetCount}) for {AudioPath}. Returning actual count.", samples.Length, sampleCount, audioPath);
                    return samples; 
                }
            }
            catch (ExternalProcessException ex)
            {
                 _logger.LogError(ex, "FFmpeg process failed during waveform data generation. ExitCode={ExitCode}, Error={Error}", ex.ExitCode, ex.ErrorMessage);
                 throw new InvalidOperationException($"Failed to generate waveform data: FFmpeg process failed (ExitCode: {ex.ExitCode}). Error: {ex.ErrorMessage}", ex);
            }
            catch (OperationCanceledException)
            {
                 _logger.LogInformation("Waveform data generation cancelled for source: {AudioPath}", audioPath);
                throw;
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An unexpected error occurred during waveform data generation for source: {AudioPath}", audioPath);
                 throw new InvalidOperationException($"An unexpected error occurred during waveform data generation: {ex.Message}", ex); 
            }
            finally
            {
                // Clean up the temporary file
                if (File.Exists(tempFilePath))
                {
                    try { File.Delete(tempFilePath); }
                    catch (IOException ioEx) { _logger.LogWarning(ioEx, "Failed to delete temporary waveform data file: {TempFilePath}", tempFilePath); }
                }
            }
        }

        /// <inheritdoc />
        public async Task<string> GenerateWaveformImageAsync(
            string audioPath,
            string outputPath,
            int width = 800,
            int height = 240,
            string foregroundColor = "#0066FF",
            string backgroundColor = "#FFFFFF",
            CancellationToken cancellationToken = default)
        {
            // Validate parameters
             if (string.IsNullOrEmpty(audioPath))
                throw new ArgumentNullException(nameof(audioPath));
            if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentNullException(nameof(outputPath));
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero.");

            // Ensure input file exists
            if (!File.Exists(audioPath))
            {
                 _logger.LogError("Waveform image source file not found: {AudioPath}", audioPath);
                throw new FileNotFoundException("The specified audio file does not exist.", audioPath);
            }

            // Ensure the output directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

            _logger.LogInformation("Generating waveform image: Source={Source}, Target={Target}, Size={W}x{H}", 
                audioPath, outputPath, width, height);

            // Verify the file has audio
            var mediaInfo = await _mediaInfoService.GetMediaInfoAsync(audioPath, cancellationToken);
            if (!mediaInfo.HasAudio)
            {
                _logger.LogWarning("Attempted to generate waveform image from file with no audio stream: {AudioPath}", audioPath);
                throw new InvalidOperationException("The specified file does not contain audio content.");
            }

            // Prepare colors for FFmpeg (handle # prefix and potentially named colors)
            string fgColorArg = PrepareColorArgument(foregroundColor);
            string bgColorArg = PrepareColorArgument(backgroundColor);

            // Build FFmpeg arguments using the showwavespic filter
            string arguments = $"-i \"{audioPath}\" -filter_complex \"" +
                                 $"aformat=channel_layouts=mono," +
                                 $"showwavespic=s={width}x{height}:colors={fgColorArg}|{bgColorArg}" +
                                 $"\" -frames:v 1 \"{outputPath}\" -y";
             _logger.LogDebug("Running FFmpeg for waveform image: {Arguments}", arguments);

            try
            {
                // Run FFmpeg process
                await _processRunner.RunProcessAsync(_ffmpegPath, arguments, cancellationToken);
                 _logger.LogInformation("Waveform image generated successfully: {OutputPath}", outputPath);
                return outputPath;
            }
            catch (ExternalProcessException ex)
            {
                 _logger.LogError(ex, "FFmpeg process failed during waveform image generation. ExitCode={ExitCode}, Error={Error}", ex.ExitCode, ex.ErrorMessage);
                 throw new InvalidOperationException($"Failed to generate waveform image: FFmpeg process failed (ExitCode: {ex.ExitCode}). Error: {ex.ErrorMessage}", ex);
            }
            catch (OperationCanceledException)
            {
                 _logger.LogInformation("Waveform image generation cancelled for source: {AudioPath}", audioPath);
                throw;
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An unexpected error occurred during waveform image generation for source: {AudioPath}", audioPath);
                 throw new InvalidOperationException($"An unexpected error occurred during waveform image generation: {ex.Message}", ex); 
            }
        }

        private string PrepareColorArgument(string color)
        {
            // Remove leading # if present
            color = color.TrimStart('#');
            // If it looks like a hex code, prefix with 0x for FFmpeg
            if (Regex.IsMatch(color, "^[0-9a-fA-F]{6}$") || Regex.IsMatch(color, "^[0-9a-fA-F]{8}$"))
            {
                return $"0x{color}";
            }
            // Otherwise, assume it's a named color and pass it directly
            return color;
        }
    }
} 
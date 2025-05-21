using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// Implementation of <see cref="IMediaConcatenatorService"/> using the FFmpeg concat demuxer.
    /// </summary>
    public class FFmpegMediaConcatenator : IMediaConcatenatorService
    {
        private readonly ILogger<FFmpegMediaConcatenator> _logger;
        private readonly IProcessRunner _processRunner;
        private readonly IMediaInfoService _mediaInfoService; // To check compatibility
        private readonly string _ffmpegPath;
        private readonly string _tempDirectory; // To store the concat list file

        public FFmpegMediaConcatenator(
            ILogger<FFmpegMediaConcatenator> logger,
            IProcessRunner processRunner,
            IMediaInfoService mediaInfoService,
            IOptions<FFmpegOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
            _mediaInfoService = mediaInfoService ?? throw new ArgumentNullException(nameof(mediaInfoService));

            if (options?.Value == null) throw new ArgumentNullException(nameof(options));
            _ffmpegPath = options.Value.FFmpegPath ?? "ffmpeg";
            _tempDirectory = options.Value.TempDirectory ?? Path.Combine(Path.GetTempPath(), "FFmpegTemp");

            if (string.IsNullOrEmpty(_ffmpegPath))
            {
                _logger.LogCritical("FFmpeg path is not configured.");
                throw new ConfigurationValidationException("The path to FFmpeg is not configured in FFmpegOptions.");
            }
             // Ensure temp directory exists
            Directory.CreateDirectory(_tempDirectory);
        }

        /// <inheritdoc />
        public async Task<string> ConcatenateMediaAsync(
            List<string> inputPaths,
            string outputPath,
            CancellationToken cancellationToken = default)
        {
            // Validate parameters
            if (inputPaths == null || !inputPaths.Any())
                throw new ArgumentNullException(nameof(inputPaths), "Input paths list cannot be null or empty.");
            if (inputPaths.Any(string.IsNullOrEmpty))
                 throw new ArgumentException("Input paths list cannot contain null or empty paths.", nameof(inputPaths));
            if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentNullException(nameof(outputPath));

             _logger.LogInformation("Starting media concatenation: Count={Count}, Target={Target}", inputPaths.Count, outputPath);

            // Validate that all input files exist
            foreach (var path in inputPaths)
            {
                if (!File.Exists(path))
                {
                     _logger.LogError("Concatenation input file not found: {InputPath}", path);
                    throw new FileNotFoundException($"The input file does not exist: {path}", path);
                }
            }

            // Ensure the output directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

            // Check basic compatibility (requires identical codecs for concat demuxer)
            // More robust checks might compare resolution, frame rate, sample rate etc.
            await ValidateCompatibilityAsync(inputPaths, cancellationToken);

            // Create a temporary concat file that lists all the input files
            string concatFilePath = Path.Combine(_tempDirectory, $"concat_{Path.GetRandomFileName()}.txt");
            _logger.LogDebug("Creating temporary concat list file: {ConcatFilePath}", concatFilePath);

            try
            {
                // Write the list of files to the temporary file
                await WriteConcatFileAsync(inputPaths, concatFilePath);

                // Build the FFmpeg arguments for concatenation using the concat demuxer
                // -safe 0 allows relative/absolute paths in the concat file
                // -c copy performs stream copy (no re-encoding)
                string arguments = $"-f concat -safe 0 -i \"{concatFilePath}\" -c copy \"{outputPath}\" -y";
                _logger.LogDebug("Running FFmpeg for concatenation: {Arguments}", arguments);

                // Execute FFmpeg
                await _processRunner.RunProcessAsync(_ffmpegPath, arguments, cancellationToken);

                 _logger.LogInformation("Media concatenation completed successfully: {OutputPath}", outputPath);
                return outputPath;
            }
            catch (ExternalProcessException ex)
            {
                 _logger.LogError(ex, "FFmpeg process failed during concatenation. ExitCode={ExitCode}, Error={Error}", ex.ExitCode, ex.ErrorMessage);
                 throw new InvalidOperationException($"Failed to concatenate media: FFmpeg process failed (ExitCode: {ex.ExitCode}). Error: {ex.ErrorMessage}", ex);
            }
            catch (OperationCanceledException)
            {
                 _logger.LogInformation("Media concatenation cancelled.");
                throw;
            }
            catch (Exception ex) when (ex is not FileNotFoundException && ex is not ArgumentException && ex is not InvalidOperationException)
            {
                 _logger.LogError(ex, "An unexpected error occurred during media concatenation.");
                 throw new InvalidOperationException($"An unexpected error occurred during media concatenation: {ex.Message}", ex); 
            }
            finally
            {
                // Clean up the temporary concat file
                if (File.Exists(concatFilePath))
                {
                    try 
                    { 
                        File.Delete(concatFilePath); 
                        _logger.LogDebug("Deleted temporary concat list file: {ConcatFilePath}", concatFilePath);
                    }
                    catch (IOException ioEx) { _logger.LogWarning(ioEx, "Failed to delete temporary concat list file: {ConcatFilePath}", concatFilePath); }
                }
            }
        }
        
        private async Task WriteConcatFileAsync(List<string> inputPaths, string concatFilePath)
        {
            // Write the list of files to a temporary file in the format required by FFmpeg
            // Example line: file '/path/to/your/file1.mp4'
            using (var writer = new StreamWriter(concatFilePath, false, System.Text.Encoding.UTF8))
            {
                foreach (var path in inputPaths)
                {
                    // FFmpeg concat demuxer requires paths to be escaped carefully.
                    // Single quotes need to be escaped as '\''
                    string escapedPath = path.Replace("'", "'\\''"); 
                    await writer.WriteLineAsync($"file '{escapedPath}'");
                }
            }
        }

        private async Task ValidateCompatibilityAsync(List<string> inputPaths, CancellationToken cancellationToken)
        {
            if (inputPaths.Count < 2) return; // No need to validate for a single file

            var firstMediaInfo = await _mediaInfoService.GetMediaInfoAsync(inputPaths[0], cancellationToken);
            bool firstHasVideo = firstMediaInfo.HasVideo;
            bool firstHasAudio = firstMediaInfo.HasAudio;
            string? firstVideoCodec = firstMediaInfo.PrimaryVideoStream?.Codec;
            string? firstAudioCodec = firstMediaInfo.PrimaryAudioStream?.Codec;
            _logger.LogDebug("Compatibility Check Base: Video={HasVideo} ({VCodec}), Audio={HasAudio} ({ACodec}) for {File}", 
                firstHasVideo, firstVideoCodec, firstHasAudio, firstAudioCodec, Path.GetFileName(inputPaths[0]));

            for (int i = 1; i < inputPaths.Count; i++)
            {
                var currentPath = inputPaths[i];
                var mediaInfo = await _mediaInfoService.GetMediaInfoAsync(currentPath, cancellationToken);

                // Check if all files have video or none do
                if (mediaInfo.HasVideo != firstHasVideo)
                {
                    _logger.LogError("Concatenation compatibility failed: File {File} video stream presence ({HasVideo}) differs from first file ({FirstHasVideo}).", 
                        Path.GetFileName(currentPath), mediaInfo.HasVideo, firstHasVideo);
                    throw new InvalidOperationException("Input files for concatenation must all have video streams or all not have video streams.");
                }

                // Check if all files have audio or none do
                if (mediaInfo.HasAudio != firstHasAudio)
                {
                    _logger.LogError("Concatenation compatibility failed: File {File} audio stream presence ({HasAudio}) differs from first file ({FirstHasAudio}).", 
                        Path.GetFileName(currentPath), mediaInfo.HasAudio, firstHasAudio);
                    throw new InvalidOperationException("Input files for concatenation must all have audio streams or all not have audio streams.");
                }

                // If they have video, check codec compatibility (crucial for concat demuxer)
                string? currentVideoCodec = mediaInfo.PrimaryVideoStream?.Codec;
                if (firstHasVideo && currentVideoCodec != firstVideoCodec)
                {
                     _logger.LogError("Concatenation compatibility failed: File {File} video codec ({CurrentCodec}) differs from first file ({FirstCodec}).", 
                        Path.GetFileName(currentPath), currentVideoCodec, firstVideoCodec);
                    throw new InvalidOperationException($"Video codec mismatch for concatenation: File {i+1} ({currentVideoCodec}) vs File 1 ({firstVideoCodec}). Concat demuxer requires identical codecs.");
                }

                // If they have audio, check codec compatibility
                string? currentAudioCodec = mediaInfo.PrimaryAudioStream?.Codec;
                if (firstHasAudio && currentAudioCodec != firstAudioCodec)
                {
                    _logger.LogError("Concatenation compatibility failed: File {File} audio codec ({CurrentCodec}) differs from first file ({FirstCodec}).", 
                        Path.GetFileName(currentPath), currentAudioCodec, firstAudioCodec);
                    throw new InvalidOperationException($"Audio codec mismatch for concatenation: File {i+1} ({currentAudioCodec}) vs File 1 ({firstAudioCodec}). Concat demuxer requires identical codecs.");
                }

                 _logger.LogDebug("Compatibility Check Pass: Video={HasVideo} ({VCodec}), Audio={HasAudio} ({ACodec}) for {File}", 
                    mediaInfo.HasVideo, currentVideoCodec, mediaInfo.HasAudio, currentAudioCodec, Path.GetFileName(currentPath));
            }
            _logger.LogInformation("Compatibility check passed for {Count} files.", inputPaths.Count);
        }
    }
} 
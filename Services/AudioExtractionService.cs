using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Infrastructure.Transcription.Configuration;

namespace TrialWorld.AssemblyAIDiagnostic.Services
{
    /// <summary>
    /// Service for extracting audio from video files.
    /// </summary>
    public interface IAudioExtractionService
    {
        /// <summary>
        /// Extracts audio from a video file.
        /// </summary>
        /// <param name="videoFilePath">The video file path.</param>
        /// <param name="outputFormat">The output format (default: mp3).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The path to the extracted audio file.</returns>
        Task<string> ExtractAudioAsync(string videoFilePath, string outputFormat = "mp3", CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Determines whether the specified file is a video file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>True if the file is a video file; otherwise, false.</returns>
        bool IsVideoFile(string filePath);
    }

    /// <summary>
    /// Implementation of the audio extraction service using FFmpeg.
    /// </summary>
    public class AudioExtractionService : IAudioExtractionService
    {
        private readonly ILogger<AudioExtractionService> _logger;
        private readonly string _ffmpegPath;
        private readonly string _extractedAudioPath;
        private readonly string[] _videoExtensions = { ".mp4", ".avi", ".mov", ".mkv", ".wmv", ".flv" };

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioExtractionService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        public AudioExtractionService(
            ILogger<AudioExtractionService> logger,
            IOptions<AssemblyAIOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Get FFmpeg path from configuration
            var config = options?.Value ?? throw new ArgumentNullException(nameof(options));
            
            // Use FFmpeg from PATH if not specified
            _ffmpegPath = config.FFmpegPath ?? "ffmpeg";
            
            // Create extracted audio directory
            _extractedAudioPath = Path.Combine(Directory.GetCurrentDirectory(), "ExtractedAudio");
            if (!Directory.Exists(_extractedAudioPath))
            {
                Directory.CreateDirectory(_extractedAudioPath);
            }
            
            _logger.LogInformation("AudioExtractionService initialized with FFmpeg path: {FFmpegPath}", _ffmpegPath);
        }

        /// <inheritdoc/>
        public async Task<string> ExtractAudioAsync(string videoFilePath, string outputFormat = "mp3", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(videoFilePath))
            {
                throw new ArgumentException("Video file path cannot be null or empty", nameof(videoFilePath));
            }

            if (!File.Exists(videoFilePath))
            {
                throw new FileNotFoundException("Video file not found", videoFilePath);
            }

            var videoFileName = Path.GetFileNameWithoutExtension(videoFilePath);
            var outputFileName = $"{videoFileName}_{DateTime.Now:yyyyMMddHHmmss}.{outputFormat}";
            var outputFilePath = Path.Combine(_extractedAudioPath, outputFileName);

            _logger.LogInformation("Extracting audio from {VideoFile} to {AudioFile}", videoFilePath, outputFilePath);

            try
            {
                // Create FFmpeg process
                var startInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = $"-i \"{videoFilePath}\" -vn -acodec {GetAudioCodec(outputFormat)} -y \"{outputFilePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };
                var outputBuilder = new System.Text.StringBuilder();
                var errorBuilder = new System.Text.StringBuilder();

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        outputBuilder.AppendLine(e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        errorBuilder.AppendLine(e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Create a task that completes when the process exits
                var processTask = Task.Run(() =>
                {
                    process.WaitForExit();
                    return process.ExitCode;
                });

                // Create a task that completes when cancellation is requested
                var cancellationTask = Task.Run(() =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        Thread.Sleep(100);
                    }
                    return -1;
                });

                // Wait for either the process to complete or cancellation
                var completedTask = await Task.WhenAny(processTask, cancellationTask).ConfigureAwait(false);
                
                if (completedTask == cancellationTask)
                {
                    // Cancellation was requested
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error killing FFmpeg process");
                    }
                    
                    cancellationToken.ThrowIfCancellationRequested();
                }

                // Check exit code
                var exitCode = await processTask.ConfigureAwait(false);
                if (exitCode != 0)
                {
                    var error = errorBuilder.ToString();
                    _logger.LogError("FFmpeg exited with code {ExitCode}. Error: {Error}", exitCode, error);
                    throw new InvalidOperationException($"FFmpeg exited with code {exitCode}. Error: {error}");
                }

                _logger.LogInformation("Audio extraction completed successfully: {OutputFile}", outputFilePath);
                return outputFilePath;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Audio extraction cancelled for {VideoFile}", videoFilePath);
                
                // Clean up partial output file
                if (File.Exists(outputFilePath))
                {
                    try
                    {
                        File.Delete(outputFilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deleting partial output file: {OutputFile}", outputFilePath);
                    }
                }
                
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting audio from {VideoFile}", videoFilePath);
                throw;
            }
        }

        /// <inheritdoc/>
        public bool IsVideoFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return Array.IndexOf(_videoExtensions, extension) >= 0;
        }

        /// <summary>
        /// Gets the audio codec for the specified output format.
        /// </summary>
        /// <param name="outputFormat">The output format.</param>
        /// <returns>The audio codec.</returns>
        private string GetAudioCodec(string outputFormat)
        {
            return outputFormat.ToLowerInvariant() switch
            {
                "mp3" => "libmp3lame",
                "aac" => "aac",
                "wav" => "pcm_s16le",
                "flac" => "flac",
                "ogg" => "libvorbis",
                _ => "copy"
            };
        }
    }
}

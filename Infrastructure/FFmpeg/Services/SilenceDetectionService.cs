using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Infrastructure.Models.FFmpeg;

namespace TrialWorld.Infrastructure.FFmpeg.Services
{
    /// <summary>
    /// Provides silence detection functionality using FFmpeg
    /// </summary>
    public class SilenceDetectionService : ISilenceDetectionService
    {
        private readonly ILogger<SilenceDetectionService> _logger;
        private readonly IProcessRunner _processRunner;
        private readonly IFFmpegPathResolver _ffmpegPathResolver;
        private readonly string _ffmpegPath;
        
        public SilenceDetectionService(
            IOptions<FFmpegOptions> options,
            IProcessRunner processRunner,
            ILogger<SilenceDetectionService> logger,
            IFFmpegPathResolver ffmpegPathResolver)
        {
            _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ffmpegPathResolver = ffmpegPathResolver ?? throw new ArgumentNullException(nameof(ffmpegPathResolver));

            if (options?.Value == null) throw new ArgumentNullException(nameof(options));
            var ffmpegOptions = options.Value;

            // Use the FFmpeg path from the resolver if available, otherwise fall back to configured path
            _ffmpegPath = ffmpegOptions.FFmpegPath ?? _ffmpegPathResolver.GetFFmpegExecutablePath();

            // Try to locate FFmpeg binary if the configured path doesn't exist
            if (!File.Exists(_ffmpegPath))
            {
                _logger.LogWarning("FFmpeg binary not found at configured path: {Path}", _ffmpegPath);
                
                // Use the resolver to find the FFmpeg path
                _ffmpegPath = _ffmpegPathResolver.GetFFmpegExecutablePath();
                
                // If the resolver's path doesn't exist, try common locations as a fallback
                if (!File.Exists(_ffmpegPath))
                {
                    _logger.LogWarning("FFmpeg binary not found at resolver path: {Path}", _ffmpegPath);
                    
                    // Try to find FFmpeg in common locations
                    string[] commonLocations = {
                        "ffmpeg.exe",                                          // Current directory
                        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe"),  // App directory
                        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg", "bin", "ffmpeg.exe"),  // Project ffmpeg directory
                        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "ffmpeg.exe"),  // Tools subdirectory
                        System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ffmpeg", "bin", "ffmpeg.exe"),
                        System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "ffmpeg", "bin", "ffmpeg.exe")
                    };
                
                    foreach (var location in commonLocations)
                    {
                        if (System.IO.File.Exists(location))
                        {
                            _ffmpegPath = location;
                            _logger.LogInformation("Found FFmpeg binary at alternate location: {Path}", _ffmpegPath);
                            break;
                        }
                    }
                    
                    if (!File.Exists(_ffmpegPath))
                    {
                        _logger.LogWarning("FFmpeg binary not found in any standard location. Silence detection will not work.");
                        _logger.LogInformation("Please run the setup-ffmpeg.ps1 script to automatically download and install FFmpeg.");
                    }
                }
            }
        }
        
        /// <summary>
        /// Detects periods of silence in an audio file using optimized FFmpeg parameters
        /// </summary>
        public async Task<List<SilencePeriod>> DetectSilenceAsync(
            string filePath, 
            int noiseFloorDb = -30,
            double minDurationSeconds = 10, 
            IProgress<int>? progress = null, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Detecting silence in {FilePath} with noise floor {NoiseFloorDb}dB and minimum duration {MinDurationSeconds}s", 
                filePath, noiseFloorDb, minDurationSeconds);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Audio file not found", filePath);
            }

            // Report initial progress
            progress?.Report(0);

            // Create a list to store silence periods
            var silencePeriods = new List<SilencePeriod>();

            try
            {
                // Build FFmpeg command to detect silence
                // Using the silencedetect filter
                string ffmpegArgs = $"-i \"{filePath}\" -af silencedetect=noise={noiseFloorDb}dB:d={minDurationSeconds} -f null -";

                _logger.LogDebug("FFmpeg silence detection command: {FFmpegPath} {FFmpegArgs}", _ffmpegPath, ffmpegArgs);

                // Create a callback to capture the stderr output for parsing
                var outputBuilder = new System.Text.StringBuilder();
                var progressHandler = new Progress<string>(line => {
                    if (!string.IsNullOrEmpty(line))
                    {
                        outputBuilder.AppendLine(line);
                        // Optionally report incremental progress based on output
                        // This is difficult to estimate precisely with FFmpeg
                    }
                });
                
                // Run the FFmpeg process through the IProcessRunner
                await _processRunner.RunProcessWithProgressAsync(
                    _ffmpegPath,
                    ffmpegArgs,
                    progressHandler,
                    cancellationToken);
                
                // Report 50% progress after FFmpeg completes
                progress?.Report(50);
                
                // Parse the output to find silence periods
                string output = outputBuilder.ToString();
                _logger.LogDebug("FFmpeg silence detection output: {Output}", output);

                // Regular expressions to extract silence start and end times
                var startRegex = new Regex(@"silence_start:\s*([\d\.]+)");
                var endRegex = new Regex(@"silence_end:\s*([\d\.]+)\s*\|\s*silence_duration:\s*([\d\.]+)");

                // Find all matches
                var startMatches = startRegex.Matches(output);
                var endMatches = endRegex.Matches(output);

                // Process matches if we have equal numbers of starts and ends
                if (startMatches.Count == endMatches.Count)
                {
                    for (int i = 0; i < startMatches.Count; i++)
                    {
                        double startTime = double.Parse(startMatches[i].Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                        double endTime = double.Parse(endMatches[i].Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                        double duration = double.Parse(endMatches[i].Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);

                        // Create a silence period
                        var silencePeriod = new SilencePeriod
                        {
                            StartTime = TimeSpan.FromSeconds(startTime),
                            EndTime = TimeSpan.FromSeconds(endTime),
                            Duration = TimeSpan.FromSeconds(duration)
                        };

                        silencePeriods.Add(silencePeriod);
                    }
                }
                else
                {
                    _logger.LogWarning("Mismatch between silence start ({StartCount}) and end ({EndCount}) markers", 
                        startMatches.Count, endMatches.Count);
                }
                
                // Report completion
                progress?.Report(100);

                _logger.LogInformation("Detected {Count} silence periods in {FilePath}", silencePeriods.Count, filePath);
                return silencePeriods;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Silence detection cancelled for {FilePath}", filePath);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting silence in {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// Removes silence from a media file and saves the result to a new file.
        /// </summary>
        public async Task<string> RemoveSilenceAsync(
            string inputFilePath,
            string outputFilePath,
            int noiseFloorDb = -30,
            double minDurationSeconds = 10,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(inputFilePath))
                throw new ArgumentException("Input file path cannot be null or empty", nameof(inputFilePath));
            if (string.IsNullOrEmpty(outputFilePath))
                throw new ArgumentException("Output file path cannot be null or empty", nameof(outputFilePath));
            if (!File.Exists(inputFilePath))
                throw new FileNotFoundException("Input file not found", inputFilePath);
            if (!File.Exists(_ffmpegPath))
            {
                _logger.LogError("Cannot perform silence removal: FFmpeg binary not available. Please install FFmpeg and configure the path.");
                progress?.Report(100);
                throw new InvalidOperationException("FFmpeg binary not available");
            }

            _logger.LogInformation("Removing silence from {InputFile} to {OutputFile} with noise floor {NoiseFloor}dB and min duration {MinDuration}s", 
                inputFilePath, outputFilePath, noiseFloorDb, minDurationSeconds);

            // Create temporary files
            string tempAudioPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".wav");
            string tempVideoPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".mp4");
            string tempAudioNoSilencePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".wav");
            string tempMergedPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".mp4");

            try
            {
                // Get duration of input file for progress estimation
                TimeSpan inputDuration = TimeSpan.Zero;
                try
                {
                    var probeOutput = await _processRunner.RunProcessAsync(
                        _ffmpegPath,
                        $"-i \"{inputFilePath}\"",
                        CancellationToken.None);
                    
                    var durationRegex = new Regex(@"Duration: (\d+):(\d+):(\d+\.\d+)");
                    var match = durationRegex.Match(probeOutput);
                    if (match.Success)
                    {
                        int hours = int.Parse(match.Groups[1].Value);
                        int minutes = int.Parse(match.Groups[2].Value);
                        double seconds = double.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture);
                        inputDuration = new TimeSpan(0, hours, minutes, (int)seconds, (int)((seconds - (int)seconds) * 1000));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to determine input file duration. Progress reporting may be inaccurate.");
                    // Continue with the process even if we can't determine the duration
                }

                // Create a cleanup helper
                var tempFiles = new List<string> { tempAudioPath, tempVideoPath, tempAudioNoSilencePath, tempMergedPath };
                void CleanupTempFiles()
                {
                    foreach (var file in tempFiles.Where(File.Exists))
                    {
                        try { File.Delete(file); }
                        catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete temporary file: {File}", file); }
                    }
                }

                // Progress tracking
                double currentProgress = 0;
                const double totalSteps = 5.0;
                
                // Helper method to update progress
                void UpdateProgress(int step, double stepProgress)
                {
                    var newProgress = ((step - 1) / totalSteps) + (stepProgress / totalSteps);
                    if (newProgress > currentProgress)
                    {
                        currentProgress = newProgress;
                        progress?.Report(currentProgress * 100);
                    }
                }

                // Step 1: Extract audio stream
                _logger.LogInformation("Extracting audio stream...");
                await _processRunner.RunProcessWithProgressAsync(
                    _ffmpegPath,
                    $"-hide_banner -y -i \"{inputFilePath}\" -vn -acodec pcm_s16le -ar 44100 -ac 2 \"{tempAudioPath}\"",
                    new Progress<string>(_ => UpdateProgress(1, 1.0)),
                    cancellationToken);
                
                // Step 2: Remove silence from audio
                _logger.LogInformation("Removing silence from audio...");
                string silenceRemoveFilter = $"silenceremove=start_periods=1:start_silence={minDurationSeconds}:start_threshold={noiseFloorDb}dB:stop_periods=1:stop_silence={minDurationSeconds}:stop_threshold={noiseFloorDb}dB:detection=peak";
                
                await _processRunner.RunProcessWithProgressAsync(
                    _ffmpegPath,
                    $"-hide_banner -y -i \"{tempAudioPath}\" -af {silenceRemoveFilter} \"{tempAudioNoSilencePath}\"",
                    new Progress<string>(_ => UpdateProgress(2, 1.0)),
                    cancellationToken);
                
                // Step 3: Extract video stream (without audio)
                _logger.LogInformation("Extracting video stream...");
                await _processRunner.RunProcessWithProgressAsync(
                    _ffmpegPath,
                    $"-hide_banner -y -i \"{inputFilePath}\" -an -c:v copy \"{tempVideoPath}\"",
                    new Progress<string>(_ => UpdateProgress(3, 1.0)),
                    cancellationToken);
                
                // Step 4: Merge processed audio with original video
                _logger.LogInformation("Merging audio and video...");
                await _processRunner.RunProcessWithProgressAsync(
                    _ffmpegPath,
                    $"-hide_banner -y -i \"{tempVideoPath}\" -i \"{tempAudioNoSilencePath}\" -c:v copy -map 0:v:0 -map 1:a:0 -shortest \"{tempMergedPath}\"",
                    new Progress<string>(_ => UpdateProgress(4, 1.0)),
                    cancellationToken);
                
                // Step 5: Copy metadata and finalize
                _logger.LogInformation("Finalizing output...");
                await _processRunner.RunProcessWithProgressAsync(
                    _ffmpegPath,
                    $"-hide_banner -y -i \"{tempMergedPath}\" -c copy -map_metadata 0 -movflags +faststart \"{outputFilePath}\"",
                    new Progress<string>(_ => UpdateProgress(5, 1.0)),
                    cancellationToken);
                
                // Verify the output file exists
                if (!File.Exists(outputFilePath))
                {
                    throw new FileNotFoundException("Output file was not created", outputFilePath);
                }
                
                progress?.Report(100);
                _logger.LogInformation("Successfully removed silence from {InputFile} to {OutputFile}", inputFilePath, outputFilePath);
                
                // Clean up temporary files before returning
                CleanupTempFiles();
                
                return outputFilePath;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Silence removal was cancelled for {InputFile}", inputFilePath);
                throw new OperationCanceledException("Silence removal was cancelled by the user.", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing silence from {InputFile}", inputFilePath);
                throw new InvalidOperationException($"Failed to remove silence: {ex.Message}", ex);
            }
            finally
            {
                // Clean up temporary files
                try
                {
                    var tempFiles = new[] { tempAudioPath, tempVideoPath, tempAudioNoSilencePath, tempMergedPath };
                    foreach (var file in tempFiles)
                    {
                        try 
                        { 
                            if (File.Exists(file)) 
                                File.Delete(file); 
                        }
                        catch (Exception ex) 
                        { 
                            _logger.LogWarning(ex, "Failed to delete temporary file: {File}", file); 
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during cleanup of temporary files");
                }
            }
        }
    }
}

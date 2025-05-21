using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Interfaces;

namespace TrialWorld.Infrastructure.Services
{
    /// <summary>
    /// Implementation of IFFmpegPathResolver that resolves paths to FFmpeg executables.
    /// </summary>
    public class FFmpegPathResolver : IFFmpegPathResolver
    {
        private readonly ILogger<FFmpegPathResolver> _logger;
        private const string FFMPEG_EXECUTABLE = "ffmpeg.exe";
        private const string FFPROBE_EXECUTABLE = "ffprobe.exe";
        private readonly string _ffmpegDirectory;

        /// <summary>
        /// Initializes a new instance of the FFmpegPathResolver class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public FFmpegPathResolver(ILogger<FFmpegPathResolver> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Check specific locations for FFmpeg in priority order
            string[] possibleLocations = {
                // 1. Check project root directory
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\..\\ffmpeg"),
                
                // 2. Check bin directory
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg"),
                
                // 3. Check bin\ffmpeg\bin directory (common structure)
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg", "bin"),
                
                // 4. Check parent of bin directory
                Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.FullName ?? string.Empty, "ffmpeg")
            };
            
            // Find the first directory that exists
            foreach (var location in possibleLocations)
            {
                var normalizedPath = Path.GetFullPath(location);
                if (Directory.Exists(normalizedPath))
                {
                    _ffmpegDirectory = normalizedPath;
                    _logger.LogInformation("Found FFmpeg directory at: {FFmpegDirectory}", _ffmpegDirectory);
                    
                    // Check if the executable exists in this directory
                    if (File.Exists(Path.Combine(_ffmpegDirectory, FFMPEG_EXECUTABLE)))
                    {
                        _logger.LogInformation("Found FFmpeg executable at: {FFmpegPath}", 
                            Path.Combine(_ffmpegDirectory, FFMPEG_EXECUTABLE));
                        break;
                    }
                    
                    // Check if there's a bin subdirectory with the executable
                    var binDir = Path.Combine(_ffmpegDirectory, "bin");
                    if (Directory.Exists(binDir) && File.Exists(Path.Combine(binDir, FFMPEG_EXECUTABLE)))
                    {
                        _ffmpegDirectory = binDir;
                        _logger.LogInformation("Found FFmpeg executable in bin subdirectory: {FFmpegPath}", 
                            Path.Combine(_ffmpegDirectory, FFMPEG_EXECUTABLE));
                        break;
                    }
                }
            }
            
            // If we haven't found it, default to the application directory
            if (!Directory.Exists(_ffmpegDirectory))
            {
                _ffmpegDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg");
                _logger.LogWarning("FFmpeg directory not found in any standard location. Using default: {FFmpegDirectory}", _ffmpegDirectory);
            }
            
            _logger.LogInformation("FFmpeg directory set to: {FFmpegDirectory}", _ffmpegDirectory);
        }

        /// <summary>
        /// Gets the path to the FFmpeg executable.
        /// </summary>
        /// <returns>The path to the FFmpeg executable.</returns>
        public string GetFFmpegExecutablePath()
        {
            // First check directly in the configured directory
            var path = Path.Combine(_ffmpegDirectory, FFMPEG_EXECUTABLE);
            if (File.Exists(path))
            {
                _logger.LogDebug("FFmpeg executable found at: {FFmpegPath}", path);
                return path;
            }
            
            // Check in the bin subdirectory
            var binPath = Path.Combine(_ffmpegDirectory, "bin", FFMPEG_EXECUTABLE);
            if (File.Exists(binPath))
            {
                _logger.LogDebug("FFmpeg executable found in bin subdirectory: {FFmpegPath}", binPath);
                return binPath;
            }
            
            // If not found, return the original path and let the caller handle the missing file
            _logger.LogWarning("FFmpeg executable not found at expected path: {FFmpegPath}", path);
            return path;
        }

        /// <summary>
        /// Gets the path to the FFprobe executable.
        /// </summary>
        /// <returns>The path to the FFprobe executable.</returns>
        public string GetFFprobePath()
        {
            // First check directly in the configured directory
            var path = Path.Combine(_ffmpegDirectory, FFPROBE_EXECUTABLE);
            if (File.Exists(path))
            {
                _logger.LogDebug("FFprobe executable found at: {FFprobePath}", path);
                return path;
            }
            
            // Check in the bin subdirectory
            var binPath = Path.Combine(_ffmpegDirectory, "bin", FFPROBE_EXECUTABLE);
            if (File.Exists(binPath))
            {
                _logger.LogDebug("FFprobe executable found in bin subdirectory: {FFprobePath}", binPath);
                return binPath;
            }
            
            // If not found, return the original path and let the caller handle the missing file
            _logger.LogWarning("FFprobe executable not found at expected path: {FFprobePath}", path);
            return path;
        }

        /// <summary>
        /// Asynchronously validates that FFmpeg is installed and accessible.
        /// </summary>
        /// <returns>True if FFmpeg is valid and accessible, otherwise false.</returns>
        public async Task<bool> ValidateFFmpegAsync()
        {
            try
            {
                var ffmpegPath = GetFFmpegExecutablePath();
                if (!File.Exists(ffmpegPath))
                {
                    _logger.LogWarning("FFmpeg executable not found at path: {FFmpegPath}", ffmpegPath);
                    return false;
                }

                // Run FFmpeg with -version to check if it's working
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = "-version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                var isValid = process.ExitCode == 0 && output.Contains("ffmpeg version");
                
                if (isValid)
                {
                    _logger.LogInformation("FFmpeg validation successful");
                }
                else
                {
                    _logger.LogWarning("FFmpeg validation failed. Exit code: {ExitCode}", process.ExitCode);
                }
                
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating FFmpeg");
                return false;
            }
        }
    }
}

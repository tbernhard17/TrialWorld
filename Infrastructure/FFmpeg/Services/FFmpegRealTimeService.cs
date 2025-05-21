using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using TrialWorld.Core.Exceptions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using TrialWorld.Core.Models;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Enums;
using TrialWorld.Core.Models.Configuration;
using TrialWorld.Core.StreamInfo;
using System.Windows.Media;
using FFmpeg.AutoGen;
using TrialWorld.Infrastructure.Models.FFmpeg;
namespace TrialWorld.Infrastructure.FFmpeg.Services
{
    public class FFmpegRealTimeService : IFFmpegRealTimeService, IAsyncDisposable
    {
        private readonly ILogger<FFmpegRealTimeService> _logger;
        private readonly IProcessRunner _processRunner;
        private readonly string _ffmpegPath;
        private readonly ConcurrentDictionary<string, Process> _activeProcesses;
        private readonly ConcurrentQueue<VideoFrame> _videoFrameQueue;
        private readonly ConcurrentQueue<AudioSamples> _audioSamplesQueue;
        private readonly SemaphoreSlim _videoFrameSemaphore;
        private readonly SemaphoreSlim _audioSamplesSemaphore;

        private Process? _ffmpegProcess;
        private AnonymousPipeServerStream? _videoOutputStream;
        private AnonymousPipeServerStream? _audioOutputStream;
        private CancellationTokenSource? _processCts;
        private Task? _videoReaderTask;
        private Task? _audioReaderTask;
        private double _playbackRate = 1.0;
        private bool _isDisposed;
        private long _droppedFrames = 0;
        private long _processedFrames;
        private System.Diagnostics.Stopwatch _latencyStopwatch;
        private readonly ConcurrentDictionary<long, DateTime> _frameTimestamps;

        public FFmpegRealTimeService(
            IOptions<FFmpegOptions> options,
            IProcessRunner processRunner,
            ILogger<FFmpegRealTimeService> logger)
        {
            _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (options?.Value == null) throw new ArgumentNullException(nameof(options));
            var ffmpegOptions = options.Value;

            _ffmpegPath = ffmpegOptions.FFmpegPath ?? "ffmpeg";

            // Validate FFmpeg binary existence at startup
            if (!System.IO.File.Exists(_ffmpegPath))
            {
                _logger.LogCritical("FFmpeg binary not found at path: {Path}", _ffmpegPath);
                throw new FileNotFoundException($"FFmpeg binary not found at path: {_ffmpegPath}", _ffmpegPath);
            }

            _activeProcesses = new ConcurrentDictionary<string, Process>();
            _videoFrameQueue = new ConcurrentQueue<VideoFrame>();
            _audioSamplesQueue = new ConcurrentQueue<AudioSamples>();
            _videoFrameSemaphore = new SemaphoreSlim(0);
            _audioSamplesSemaphore = new SemaphoreSlim(0);
            _latencyStopwatch = new System.Diagnostics.Stopwatch();
            _frameTimestamps = new ConcurrentDictionary<long, DateTime>();
        }

        public bool IsProcessing => _activeProcesses.Count > 0;
        public long DroppedFrames => _droppedFrames;
        public long ProcessedFrames => _processedFrames;

        public double PlaybackRate
        {
            get => _playbackRate;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Playback rate must be positive");
                _playbackRate = value;
                UpdatePlaybackRate();
            }
        }

        public async Task<bool> InitializeAsync(
            string inputPath, 
            HardwareAccelerationMode accelerationMode, 
            CancellationToken cancellationToken = default)
        {
            if (_ffmpegProcess != null)
                throw new InvalidOperationException("FFmpeg process is already running");

            try
            {
                // Create pipes for video and audio output
                _videoOutputStream = new AnonymousPipeServerStream(PipeDirection.In);
                _audioOutputStream = new AnonymousPipeServerStream(PipeDirection.In);

                // Build FFmpeg arguments for processing with performance settings
                var performanceConfig = new TranscriptionPerformanceConfig
                {
                    HardwareAcceleration = accelerationMode,
                    Threads = 0, // Default to auto
                    QualityPreset = "medium" // Default preset
                };
                
                // Build hardware acceleration arguments based on mode
                string hwaccel = GetHwAccelArguments(performanceConfig.HardwareAcceleration);
                
                // Add thread count for CPU processing if specified (0 = auto)
                string threadCount = performanceConfig.Threads > 0 ? $" -threads {performanceConfig.Threads}" : string.Empty;
                
                // Set quality preset if specified
                string preset = !string.IsNullOrEmpty(performanceConfig.QualityPreset) ? $" -preset {performanceConfig.QualityPreset}" : string.Empty;

                // Just use the standard name directly instead of trying to get it from FFmpeg
                string pixelFormatName = "rgb24";

                // Additional arguments (can be empty)
                string additionalArgs = string.Empty;

                var arguments = $"{hwaccel}{threadCount}{preset} -i \"{inputPath}\" " +
                              $"-f rawvideo -pix_fmt {pixelFormatName} -vsync 0 -c:v rawvideo " +
                              $"{additionalArgs} -";

                _logger.LogInformation("FFmpeg starting with performance settings: Acceleration={Acceleration}, Threads={Threads}, Preset={Preset}", 
                    performanceConfig.HardwareAcceleration, performanceConfig.Threads, performanceConfig.QualityPreset);

                // Start FFmpeg process
                _ffmpegProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _ffmpegPath,
                        Arguments = arguments,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true
                    }
                };

                _processCts = new CancellationTokenSource();
                if (!_ffmpegProcess.Start())
                    throw new FFmpegProcessingException("Failed to start FFmpeg process");

                _logger.LogInformation("Started FFmpeg real-time process: {Command} {Args}", _ffmpegPath, arguments);

                // Start reader tasks
                _videoReaderTask = ReadVideoFramesAsync(_processCts.Token);
                _audioReaderTask = ReadAudioSamplesAsync(_processCts.Token);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize FFmpeg process");
                await DisposeAsync();
                return false;
            }
        }

        public async Task StartStreamProcessingAsync(
            string inputUrl,
            string outputUrl,
            VideoFilterChain? videoFilters = null,
            AudioFilterChain? audioFilters = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var processId = Guid.NewGuid().ToString();
                var filterChain = BuildFilterChain(videoFilters, audioFilters);
                var startInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = BuildStreamingArguments(inputUrl, outputUrl, filterChain),
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                var process = new Process { StartInfo = startInfo };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        _logger.LogDebug("FFmpeg: {Data}", e.Data);
                    }
                };

                if (!process.Start())
                {
                    throw new FFmpegProcessingException("Failed to start FFmpeg process");
                }

                _logger.LogInformation("Started FFmpeg stream processing with ID {ProcessId}: {Command} {Args}", processId, _ffmpegPath, startInfo.Arguments);

                process.BeginErrorReadLine();

                if (!_activeProcesses.TryAdd(processId, process))
                {
                    throw new FFmpegProcessingException("Failed to track FFmpeg process");
                }

                // Start monitoring the process properly
                // We intentionally don't await here as we want this to run in the background
                // But we'll handle it properly with ConfigureAwait to avoid suppressing exceptions
                _ = MonitorProcessAsync(processId, process, cancellationToken)
                    .ConfigureAwait(false); // Explicitly handle the background task

                // Add an await operation to ensure this method properly uses async
                await Task.Yield(); // Minimal await to satisfy the async contract
                
                _logger.LogInformation("Started FFmpeg stream processing with ID {ProcessId}", processId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting FFmpeg stream processing");
                throw;
            }
        }

        public async Task StopStreamProcessingAsync(CancellationToken cancellationToken = default)
        {
            foreach (var processId in _activeProcesses.Keys)
            {
                if (_activeProcesses.TryRemove(processId, out var process))
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            // Send 'q' command to gracefully stop FFmpeg
                            process.StandardInput.Write('q');
                            await process.WaitForExitAsync(cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error stopping FFmpeg process {ProcessId}", processId);
                        try
                        {
                            process.Kill(true);
                            _logger.LogInformation("Killed FFmpeg process {ProcessId}", processId);
                        }
                        catch (Exception killEx)
                        {
                            _logger.LogError(killEx, "Error killing FFmpeg process {ProcessId}", processId);
                        }
                    }
                    finally
                    {
                        process.Dispose();
                    }

                    _logger.LogInformation("Stopped FFmpeg stream processing with ID {ProcessId}", processId);
                }
            }
        }

        public void UpdateVideoFilters(VideoFilterChain filters)
        {
            if (filters == null)
                throw new ArgumentNullException(nameof(filters));

            // If we have an active process, update its filter complex
            foreach (var process in _activeProcesses.Values)
            {
                if (!process.HasExited)
                {
                    try
                    {
                        // Send filter update command to FFmpeg
                        var filterCommand = $"filter_complex {BuildFilterString(filters)}\n";
                        process.StandardInput.Write(filterCommand);
                        process.StandardInput.Flush();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating video filters");
                    }
                }
            }
        }

        public void UpdateAudioFilters(AudioFilterChain filters)
        {
            if (filters == null)
                throw new ArgumentNullException(nameof(filters));

            // If we have an active process, update its filter complex
            foreach (var process in _activeProcesses.Values)
            {
                if (!process.HasExited)
                {
                    try
                    {
                        // Send filter update command to FFmpeg
                        var filterCommand = $"filter_complex {BuildAudioFilterString(filters)}\n";
                        process.StandardInput.Write(filterCommand);
                        process.StandardInput.Flush();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating audio filters");
                    }
                }
            }
        }

        public async Task<VideoFrame> GetNextFrameAsync(
            VideoFilterChain? filters = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _videoFrameSemaphore.WaitAsync(cancellationToken);

                if (_videoFrameQueue.TryDequeue(out var frame))
                {
                    if (filters != null)
                    {
                        // Apply filters in real-time
                        frame = await ApplyVideoFiltersAsync(frame, filters);
                    }
                    _processedFrames++;
                    return frame;
                }

                // Return empty frame if queue is empty rather than null
                return new VideoFrame
                {
                    Width = 1,
                    Height = 1,
                    PixelFormat = "rgb24",
                    Data = new byte[3],
                    Timestamp = TimeSpan.Zero,
                    PresentationTimestamp = TimeSpan.Zero,
                    IsKeyFrame = false,
                    PictureType = ' ',
                    Quality = 0
                };
            }
            catch (OperationCanceledException)
            {
                // Return empty frame on cancellation
                return new VideoFrame
                {
                    Width = 1,
                    Height = 1,
                    PixelFormat = "rgb24",
                    Data = new byte[3],
                    Timestamp = TimeSpan.Zero,
                    PresentationTimestamp = TimeSpan.Zero,
                    IsKeyFrame = false,
                    PictureType = ' ',
                    Quality = 0
                };
            }
        }

        public async Task<AudioSamples> GetNextAudioSamplesAsync(
            AudioFilterChain? filters = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _audioSamplesSemaphore.WaitAsync(cancellationToken);

                if (_audioSamplesQueue.TryDequeue(out var samples))
                {
                    if (filters != null)
                    {
                        // Apply filters in real-time
                        samples = await ApplyAudioFiltersAsync(samples, filters);
                    }
                    return samples;
                }

                // Return empty samples if queue is empty rather than null
                return new AudioSamples
                {
                    SampleRate = 48000,
                    Channels = 2,
                    Data = new byte[4],
                    Timestamp = TimeSpan.Zero,
                    PresentationTimestamp = TimeSpan.Zero,
                    SampleFormat = "s16",
                    ChannelLayout = "stereo",
                    SamplesPerChannel = 2
                };
            }
            catch (OperationCanceledException)
            {
                // Return empty samples on cancellation
                return new AudioSamples
                {
                    SampleRate = 48000,
                    Channels = 2,
                    Data = new byte[4],
                    Timestamp = TimeSpan.Zero,
                    PresentationTimestamp = TimeSpan.Zero,
                    SampleFormat = "s16",
                    ChannelLayout = "stereo",
                    SamplesPerChannel = 2
                };
            }
        }

        public async Task<bool> SeekAsync(
            TimeSpan position,
            SeekMode mode = SeekMode.Accurate,
            CancellationToken cancellationToken = default)
        {
            if (_ffmpegProcess == null)
                return false;

            try
            {
                var seekMode = mode switch
                {
                    SeekMode.FastBackward => "-seek_backward 1",
                    SeekMode.FastForward => "-seek_forward 1",
                    _ => string.Empty
                };

                // Send seek command to FFmpeg
                var seekCommand = $"seek {position.TotalSeconds}\n";
                await _ffmpegProcess.StandardInput.WriteAsync(seekCommand);
                await _ffmpegProcess.StandardInput.FlushAsync();

                // Clear queues
                while (_videoFrameQueue.TryDequeue(out _)) { }
                while (_audioSamplesQueue.TryDequeue(out _)) { }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeking in media");
                return false;
            }
        }

        public async Task<double> GetCurrentLatencyAsync()
        {
            // Add a minimal await to satisfy the async contract
            await Task.Yield();
            
            if (!_latencyStopwatch.IsRunning)
                return 0;

            // Calculate average latency based on frame timestamps
            if (_frameTimestamps.Count == 0)
                return 0;

            var now = DateTime.UtcNow;
            var totalLatency = 0.0;
            int count = 0;

            foreach (var timestamp in _frameTimestamps.Values)
            {
                totalLatency += (now - timestamp).TotalMilliseconds;
                count++;
            }

            return count > 0 ? totalLatency / count : 0;
        }

        public async Task<HardwareAccelerationInfo> GetHardwareAccelerationInfoAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Run FFmpeg to get hardware acceleration info
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _ffmpegPath,
                        Arguments = "-hwaccels",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardError.ReadToEndAsync(cancellationToken);
                await process.WaitForExitAsync(cancellationToken);

                // Parse hardware acceleration capabilities
                var supportedModes = new List<HardwareAccelerationMode> { HardwareAccelerationMode.Auto };
                var availableDecoders = new List<string>();
                var availableEncoders = new List<string>();
                var capabilities = new Dictionary<string, string>();

                // Parse hwaccels output
                if (output.Contains("cuda", StringComparison.OrdinalIgnoreCase))
                    supportedModes.Add(HardwareAccelerationMode.CUDA);
                if (output.Contains("nvenc", StringComparison.OrdinalIgnoreCase))
                    supportedModes.Add(HardwareAccelerationMode.NVENC);
                if (output.Contains("qsv", StringComparison.OrdinalIgnoreCase))
                    supportedModes.Add(HardwareAccelerationMode.QuickSync);
                if (output.Contains("amf", StringComparison.OrdinalIgnoreCase))
                    supportedModes.Add(HardwareAccelerationMode.AMF);
                if (output.Contains("vaapi", StringComparison.OrdinalIgnoreCase))
                    supportedModes.Add(HardwareAccelerationMode.VAAPI);
                if (output.Contains("videotoolbox", StringComparison.OrdinalIgnoreCase))
                    supportedModes.Add(HardwareAccelerationMode.VideoToolbox);

                // Get available encoders
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _ffmpegPath,
                        Arguments = "-encoders",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
                await process.WaitForExitAsync(cancellationToken);

                // Parse encoders (simplified for brevity)
                foreach (var line in output.Split('\n'))
                {
                    if (line.Contains("_nvenc") || line.Contains("_qsv") ||
                        line.Contains("_amf") || line.Contains("_vaapi") ||
                        line.Contains("_videotoolbox"))
                    {
                        var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 1)
                            availableEncoders.Add(parts[1]);
                    }
                }

                // Get available decoders
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _ffmpegPath,
                        Arguments = "-decoders",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
                await process.WaitForExitAsync(cancellationToken);

                // Parse decoders (simplified for brevity)
                foreach (var line in output.Split('\n'))
                {
                    if (line.Contains("_cuvid") || line.Contains("_qsv") ||
                        line.Contains("_amf") || line.Contains("_vaapi") ||
                        line.Contains("_videotoolbox"))
                    {
                        var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 1)
                            availableDecoders.Add(parts[1]);
                    }
                }

                // Add system info to capabilities
                capabilities.Add("CPU", Environment.ProcessorCount.ToString());
                capabilities.Add("OS", Environment.OSVersion.ToString());
                capabilities.Add("FFmpeg", await GetFFmpegVersionAsync(cancellationToken));

                return new HardwareAccelerationInfo
                {
                    SupportedModes = supportedModes.ToArray(),
                    AvailableDecoders = availableDecoders.ToArray(),
                    AvailableEncoders = availableEncoders.ToArray(),
                    Capabilities = capabilities
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hardware acceleration info");

                // Return basic info if we encounter an error
                return new HardwareAccelerationInfo
                {
                    SupportedModes = new[] { HardwareAccelerationMode.Auto },
                    AvailableDecoders = Array.Empty<string>(),
                    AvailableEncoders = Array.Empty<string>(),
                    Capabilities = new Dictionary<string, string>
                    {
                        { "Error", ex.Message }
                    }
                };
            }
        }

        public async Task DisposeAsync()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            try
            {
                // Cancel processing
                _processCts?.Cancel();

                // Wait for reading tasks to complete
                if (_videoReaderTask != null)
                    await _videoReaderTask;

                if (_audioReaderTask != null)
                    await _audioReaderTask;

                // Close pipes
                _videoOutputStream?.Dispose();
                _audioOutputStream?.Dispose();

                // Terminate FFmpeg process if still running
                if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
                {
                    try
                    {
                        _ffmpegProcess.Kill();
                        _logger.LogInformation("Killed FFmpeg process");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error terminating FFmpeg process");
                    }
                    _ffmpegProcess.Dispose();
                }

                // Stop all active processes
                await StopStreamProcessingAsync();

                // Dispose of semaphores
                _videoFrameSemaphore.Dispose();
                _audioSamplesSemaphore.Dispose();
                _processCts?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing FFmpeg service");
            }
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await DisposeAsync();
        }

        private async Task ReadVideoFramesAsync(CancellationToken cancellationToken)
        {
            if (_videoOutputStream == null)
                return;
                
            try
            {
                using var reader = new BinaryReader(_videoOutputStream);
                
                // Implementation for reading video frames from pipe
                // This is a placeholder implementation
                await Task.Delay(100, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Task was canceled, just exit
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading video frames");
            }
        }

        private async Task ReadAudioSamplesAsync(CancellationToken cancellationToken)
        {
            if (_audioOutputStream == null)
                return;
                
            try
            {
                using var reader = new BinaryReader(_audioOutputStream);
                
                // Implementation for reading audio samples from pipe
                // This is a placeholder implementation
                await Task.Delay(100, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Task was canceled, just exit
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading audio samples");
            }
        }

        private async Task<VideoFrame> ApplyVideoFiltersAsync(VideoFrame frame, VideoFilterChain filters)
        {
            if (frame == null || filters == null)
                return frame ?? new VideoFrame {
                    Data = Array.Empty<byte>(),
                    Width = 0,
                    Height = 0,
                    PixelFormat = "unknown",
                    Timestamp = TimeSpan.Zero,
                    PresentationTimestamp = TimeSpan.Zero
                }; // Return empty but valid frame instead of null
                
            // Apply filters to video frame
            // This is a placeholder implementation
            await Task.Delay(1);
            return frame;
        }

        private async Task<AudioSamples> ApplyAudioFiltersAsync(AudioSamples samples, AudioFilterChain filters)
        {
            if (samples == null || filters == null)
                return samples ?? new AudioSamples {
                    Data = Array.Empty<byte>(),
                    SampleRate = 0,
                    Channels = 0,
                    SampleFormat = "unknown",
                    Timestamp = TimeSpan.Zero,
                    PresentationTimestamp = TimeSpan.Zero,
                    SamplesPerChannel = 0
                }; // Return empty but valid samples instead of null
                
            // Apply filters to audio samples
            // This is a placeholder implementation
            await Task.Delay(1);
            return samples;
        }

        private void UpdatePlaybackRate()
        {
            if (_ffmpegProcess == null || _ffmpegProcess.HasExited)
                return;
                
            try
            {
                // Update playback rate in active FFmpeg process
                // This is a placeholder implementation
                _logger.LogDebug("Setting playback rate to {Rate}", _playbackRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating playback rate");
            }
        }

        private string BuildStreamingArguments(string inputUrl, string outputUrl, string filterChain)
        {
            if (string.IsNullOrEmpty(inputUrl))
                throw new ArgumentNullException(nameof(inputUrl));
                
            if (string.IsNullOrEmpty(outputUrl))
                throw new ArgumentNullException(nameof(outputUrl));
                
            // Build FFmpeg arguments for streaming
            return $"-i \"{inputUrl}\" {filterChain} \"{outputUrl}\"";
        }

        private string BuildFilterChain(VideoFilterChain? videoFilters, AudioFilterChain? audioFilters)
        {
            var filterParts = new List<string>();
            
            if (videoFilters != null && videoFilters.GetFilters().Any())
            {
                filterParts.Add(BuildFilterString(videoFilters));
            }
            
            if (audioFilters != null && audioFilters.GetFilters().Any())
            {
                filterParts.Add(BuildAudioFilterString(audioFilters));
            }
            
            return filterParts.Count > 0 ? $"-filter_complex \"{string.Join(';', filterParts)}\"" : "";
        }

        private string BuildFilterString(VideoFilterChain filters)
        {
            if (filters == null || !filters.GetFilters().Any())
                return "";
                
            // Build video filter string
            return string.Join(",", filters.GetFilters().Select(f => $"{f.Name}={f.Value}"));
        }

        private string BuildAudioFilterString(AudioFilterChain filters)
        {
            if (filters == null || !filters.GetFilters().Any())
                return "";
                
            // Build audio filter string
            return string.Join(",", filters.GetFilters().Select(f => $"{f.Name}={f.Value}"));
        }

        private async Task MonitorProcessAsync(string processId, Process process, CancellationToken cancellationToken)
        {
            if (process == null)
                return;
                
            try
            {
                // Wait for process to exit
                await process.WaitForExitAsync(cancellationToken);
                
                // Remove from active processes
                _activeProcesses.TryRemove(processId, out _);
                
                _logger.LogInformation("FFmpeg process {ProcessId} exited with code {ExitCode}",
                    processId, process.ExitCode);
            }
            catch (OperationCanceledException)
            {
                // Task was canceled, just exit
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring FFmpeg process {ProcessId}", processId);
            }
            finally
            {
                process.Dispose();
            }
        }

        private async Task<string> GetFFmpegVersionAsync(CancellationToken cancellationToken)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _ffmpegPath,
                        Arguments = "-version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                
                process.Start();
                var output = await process.StandardOutput.ReadLineAsync();
                await process.WaitForExitAsync(cancellationToken);
                
                return output ?? "Unknown";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting FFmpeg version");
                return "Unknown";
            }
        }
        

        
        /// <summary>
        /// Get FFmpeg hardware acceleration arguments based on the selected mode
        /// </summary>
        private string GetHwAccelArguments(HardwareAccelerationMode mode)
        {
            switch (mode)
            {
                case HardwareAccelerationMode.CUDA:
                    return " -hwaccel cuda";
                case HardwareAccelerationMode.NVENC:
                    return " -hwaccel cuda -c:v h264_nvenc";
                case HardwareAccelerationMode.QuickSync:
                    return " -hwaccel qsv";
                case HardwareAccelerationMode.AMF:
                    return " -hwaccel amf";
                case HardwareAccelerationMode.VAAPI:
                    return " -hwaccel vaapi";
                case HardwareAccelerationMode.VideoToolbox:
                    return " -hwaccel videotoolbox";
                case HardwareAccelerationMode.Auto:
                    // Try to detect the best acceleration based on the system
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        // On Windows, try NVENC first, then QuickSync, then AMF
                        return " -hwaccel auto";
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        return " -hwaccel videotoolbox";
                    }
                    else
                    {
                        // Linux - try VAAPI
                        return " -hwaccel vaapi";
                    }
                case HardwareAccelerationMode.None:
                default:
                    return string.Empty; // No hardware acceleration
            }
        }
    }
}
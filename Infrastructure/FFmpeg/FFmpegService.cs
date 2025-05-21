using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using TrialWorld.Core.StreamInfo;
using TrialWorld.Core.Exceptions;
using TrialWorld.Core.Models.Processing;
using TrialWorld.Core.Models.Analysis;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Services;
using TrialWorld.Core.Models;
using TrialWorld.Infrastructure.Models.FFmpeg;

namespace TrialWorld.Infrastructure.FFmpeg
{
    /// <summary>
    /// Implementation of the FFmpeg service that interacts with the FFmpeg command-line tools
    /// to provide media processing capabilities.
    /// </summary>
    public class FFmpegService : IFFmpegService
    {
        private readonly string _ffmpegPath;
        private readonly string _ffplayPath;
        private readonly string _tempDirectory;
        private MediaProcessingStatus _currentStatus = MediaProcessingStatus.Unknown;
        private double _currentProgress;
        private CancellationTokenSource? _cancellationTokenSource = new CancellationTokenSource();
        private readonly ILogger<FFmpegService> _logger;
        private readonly IProcessRunner _processRunner;
        private readonly IMediaInfoService _mediaInfoService;

        public event EventHandler<ProcessingEventArgs>? StatusChanged;

        public MediaProcessingStatus CurrentStatus
        {
            get => _currentStatus;
            private set
            {
                if (_currentStatus != value)
                {
                    _currentStatus = value;
                    OnStatusChanged(new ProcessingEventArgs(nameof(FFmpegService), value, _currentProgress));
                }
            }
        }

        public double CurrentProgress
        {
            get => _currentProgress;
            private set
            {
                if (_currentProgress != value)
                {
                    _currentProgress = value;
                    OnStatusChanged(new ProcessingEventArgs(nameof(FFmpegService), _currentStatus, value));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FFmpegService"/> class.
        /// </summary>
        /// <param name="options">FFmpeg options.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="processRunner">Process runner utility.</param>
        /// <param name="mediaInfoService">Media info service.</param>
        public FFmpegService(
            ILogger<FFmpegService> logger,
            IOptions<FFmpegOptions> options,
            IProcessRunner processRunner,
            IMediaInfoService mediaInfoService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
            _mediaInfoService = mediaInfoService ?? throw new ArgumentNullException(nameof(mediaInfoService));

            if (options?.Value == null) throw new ArgumentNullException(nameof(options));
            _ffmpegPath = options.Value.FFmpegPath ?? "ffmpeg";
            _ffplayPath = options.Value.FFplayPath ?? "ffplay";
            _tempDirectory = options.Value.TempDirectory ?? Path.Combine(Path.GetTempPath(), "FFmpegTemp");

            if (!Directory.Exists(_tempDirectory))
            {
                Directory.CreateDirectory(_tempDirectory);
            }

            // Try to locate FFmpeg binary if the configured path doesn't exist
            if (!File.Exists(_ffmpegPath))
            {
                _logger.LogWarning("FFmpeg binary not found at configured path: {Path}", _ffmpegPath);
                
                // Try to find FFmpeg in common locations
                string[] commonLocations = {
                    "ffmpeg.exe",                                          // Current directory
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe"),  // App directory
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "ffmpeg.exe"),  // Tools subdirectory
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ffmpeg", "bin", "ffmpeg.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "ffmpeg", "bin", "ffmpeg.exe")
                };
                
                foreach (var location in commonLocations)
                {
                    if (File.Exists(location))
                    {
                        _ffmpegPath = location;
                        _logger.LogInformation("Found FFmpeg binary at alternate location: {Path}", _ffmpegPath);
                        break;
                    }
                }
                
                // Just log a warning instead of throwing - this allows the app to at least start up
                if (!File.Exists(_ffmpegPath))
                {
                    _logger.LogWarning("FFmpeg binary not found in any standard location. Some functionality will be limited.");
                }
            }
        }

        private void ValidateOptions(FFmpegOptions options)
        {
            if (string.IsNullOrEmpty(options.BinaryFolder) || !Directory.Exists(options.BinaryFolder))
            {
                throw new ConfigurationValidationException("The BinaryFolder path is invalid or does not exist.");
            }

            if (string.IsNullOrEmpty(options.OutputDirectory) || !Directory.Exists(options.OutputDirectory))
            {
                throw new ConfigurationValidationException("The OutputDirectory path is invalid or does not exist.");
            }

            if (string.IsNullOrEmpty(options.ThumbnailDirectory) || !Directory.Exists(options.ThumbnailDirectory))
            {
                throw new ConfigurationValidationException("The ThumbnailDirectory path is invalid or does not exist.");
            }

            if (string.IsNullOrEmpty(options.DefaultVideoCodec))
            {
                throw new ConfigurationValidationException("DefaultVideoCodec is not configured.");
            }

            if (string.IsNullOrEmpty(options.DefaultAudioCodec))
            {
                throw new ConfigurationValidationException("DefaultAudioCodec is not configured.");
            }

            if (string.IsNullOrEmpty(options.DefaultContainerFormat))
            {
                throw new ConfigurationValidationException("DefaultContainerFormat is not configured.");
            }
        }

        /// <inheritdoc />
        public async Task<string> GetFFmpegVersionAsync()
        {
            try
            {
                return await _processRunner.RunProcessAsync(_ffmpegPath, "-version", CancellationToken.None);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get FFmpeg version: {ex.Message}", ex);
            }
        }

        protected virtual void OnStatusChanged(ProcessingEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        public Task CancelAsync()
        {
            if (_cancellationTokenSource == null) throw new InvalidOperationException("Cancellation token source is null");
            _cancellationTokenSource.Cancel();
            CurrentStatus = MediaProcessingStatus.Cancelled;
            return Task.CompletedTask;
        }
    }
}
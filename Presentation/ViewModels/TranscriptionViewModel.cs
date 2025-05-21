using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models.Transcription;
using TrialWorld.Presentation.Commands;

namespace TrialWorld.Presentation.ViewModels
{
    /// <summary>
    /// ViewModel for transcription UI that integrates with the enhanced transcription service
    /// </summary>
    public class TranscriptionViewModel : INotifyPropertyChanged
    {
        #region Fields
        private readonly ITranscriptionService _transcriptionService;
        private readonly ILogger<TranscriptionViewModel> _logger;
        private string _mediaPath = string.Empty;
        private bool _isTranscribing;
        private int _transcriptionProgress;
        private string _statusMessage = "Ready";
        private string _errorMessage = string.Empty;
        private string? _currentTranscriptionId;
        private CancellationTokenSource? _cancellationTokenSource;
        private ObservableCollection<TranscriptionQueueItem> _queueItems;
        private bool _notifyingQueueItems;
        private bool _enableSilenceDetection = true;
        private int _silenceThresholdDb = -30; // Default to -30dB (hardware-optimized)
        private int _minimumSilenceDurationMs = 10000; // Default to 10 seconds (hardware-optimized)
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the current media file path
        /// </summary>
        public string MediaPath
        {
            get => _mediaPath;
            set
            {
                if (SetProperty(ref _mediaPath, value))
                {
                    OnPropertyChanged(nameof(HasMedia));
                    OnPropertyChanged(nameof(CanTranscribe));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether a media file is loaded
        /// </summary>
        public bool HasMedia => !string.IsNullOrEmpty(_mediaPath);

        /// <summary>
        /// Gets a value indicating whether the media can be transcribed
        /// </summary>
        public bool CanTranscribe => HasMedia && !IsTranscribing;

        /// <summary>
        /// Gets or sets a value indicating whether transcription is in progress
        /// </summary>
        public bool IsTranscribing
        {
            get => _isTranscribing;
            set
            {
                if (SetProperty(ref _isTranscribing, value))
                {
                    OnPropertyChanged(nameof(CanTranscribe));
                    OnPropertyChanged(nameof(CanCancel));
                }
            }
        }

        /// <summary>
        /// Gets or sets the transcription progress (0-100)
        /// </summary>
        public int TranscriptionProgress
        {
            get => _transcriptionProgress;
            set => SetProperty(ref _transcriptionProgress, value);
        }

        /// <summary>
        /// Gets or sets the status message
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetProperty(ref _errorMessage, value))
                {
                    OnPropertyChanged(nameof(HasError));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether there is an error
        /// </summary>
        public bool HasError => !string.IsNullOrEmpty(_errorMessage);

        /// <summary>
        /// Gets a value indicating whether the current operation can be canceled
        /// </summary>
        public bool CanCancel => IsTranscribing && _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested;

        /// <summary>
        /// Gets the collection of transcription queue items
        /// </summary>
        public ObservableCollection<TranscriptionQueueItem> QueueItems => _queueItems;
        
        /// <summary>
        /// Gets a value indicating whether there are items in the queue
        /// </summary>
        public bool HasQueueItems => QueueItems.Count > 0;

        /// <summary>
        /// Gets or sets whether silence detection is enabled for transcription
        /// </summary>
        public bool EnableSilenceDetection
        {
            get => _enableSilenceDetection;
            set => SetProperty(ref _enableSilenceDetection, value);
        }

        /// <summary>
        /// Gets or sets the silence threshold in dB (default: -30dB)
        /// </summary>
        public int SilenceThresholdDb
        {
            get => _silenceThresholdDb;
            set => SetProperty(ref _silenceThresholdDb, value);
        }

        /// <summary>
        /// Gets or sets the minimum silence duration in milliseconds (default: 10000ms/10s)
        /// </summary>
        public int MinimumSilenceDurationMs
        {
            get => _minimumSilenceDurationMs;
            set => SetProperty(ref _minimumSilenceDurationMs, value);
        }
        #endregion

        #region Commands
        /// <summary>
        /// Gets the command to start transcription
        /// </summary>
        public ICommand TranscribeCommand { get; }

        /// <summary>
        /// Gets the command to cancel transcription
        /// </summary>
        public ICommand CancelTranscriptionCommand { get; }
        
        /// <summary>
        /// Gets or sets the command to browse for media files
        /// </summary>
        public ICommand BrowseMediaCommand { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the command to open a completed transcription
        /// </summary>
        public ICommand OpenTranscriptionCommand { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the command to retry a failed transcription
        /// </summary>
        public ICommand RetryTranscriptionCommand { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the command to clear error messages
        /// </summary>
        public ICommand ClearErrorCommand { get; set; } = null!;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="TranscriptionViewModel"/> class
        /// </summary>
        /// <param name="transcriptionService">The transcription service</param>
        /// <param name="logger">The logger</param>
        public TranscriptionViewModel(ITranscriptionService transcriptionService, ILogger<TranscriptionViewModel> logger)
        {
            _transcriptionService = transcriptionService ?? throw new ArgumentNullException(nameof(transcriptionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Initialize the queue collection with change notification
            _queueItems = new ObservableCollection<TranscriptionQueueItem>();
            _queueItems.CollectionChanged += (sender, e) => 
            {
                if (!_notifyingQueueItems)
                {
                    _notifyingQueueItems = true;
                    OnPropertyChanged(nameof(HasQueueItems));
                    _notifyingQueueItems = false;
                }
            };

            TranscribeCommand = new RelayCommand(() => ExecuteTranscribeCommand(null), () => CanExecuteTranscribeCommand(null));
            CancelTranscriptionCommand = new RelayCommand(() => ExecuteCancelCommand(null), () => CanExecuteCancelCommand(null));
        }
        #endregion

        #region Command Handlers
        private bool CanExecuteTranscribeCommand(object? parameter) => CanTranscribe;

        private async void ExecuteTranscribeCommand(object? parameter)
        {
            if (string.IsNullOrEmpty(MediaPath))
            {
                ErrorMessage = "No media file selected";
                return;
            }

            try
            {
                IsTranscribing = true;
                ErrorMessage = string.Empty;
                TranscriptionProgress = 0;
                StatusMessage = "Starting transcription...";

                _cancellationTokenSource = new CancellationTokenSource();
                var progress = new Progress<TranscriptionProgressUpdate>(OnTranscriptionProgressUpdate);

                // Create transcription config with silence detection settings
                var config = new TranscriptionConfig
                {
                    EnableSilenceDetection = EnableSilenceDetection,
                    SilenceThresholdDb = SilenceThresholdDb,
                    MinimumSilenceDurationMs = MinimumSilenceDurationMs
                };

                // Start transcription with progress reporting
                var result = await _transcriptionService.TranscribeWithProgressAsync(
                    MediaPath,
                    config,
                    progress,
                    _cancellationTokenSource.Token);

                _currentTranscriptionId = result.TranscriptId;

                // Add to queue for tracking
                var queueItem = new TranscriptionQueueItem
                {
                    MediaPath = MediaPath,
                    TranscriptionId = result.TranscriptId,
                    Status = result.Status,
                    SubmittedAt = DateTime.Now
                };

                QueueItems.Add(queueItem);

                if (result.Status == TranscriptionStatus.Completed)
                {
                    StatusMessage = "Transcription completed successfully";
                    TranscriptionProgress = 100;
                }
                else if (result.Status == TranscriptionStatus.Processing)
                {
                    StatusMessage = "Transcription submitted and processing";
                    // Start polling for status updates
                    _ = PollTranscriptionStatusAsync(result.TranscriptId, queueItem);
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Transcription canceled";
                _logger.LogInformation("Transcription canceled for {MediaPath}", MediaPath);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Transcription failed: {ex.Message}";
                StatusMessage = "Transcription failed";
                _logger.LogError(ex, "Error during transcription for {MediaPath}", MediaPath);
            }
            finally
            {
                IsTranscribing = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private bool CanExecuteCancelCommand(object? parameter) => CanCancel;

        private void ExecuteCancelCommand(object? parameter)
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                StatusMessage = "Cancelling transcription...";
                _cancellationTokenSource.Cancel();

                // If we have a transcription ID, try to cancel it on the server as well
                if (!string.IsNullOrEmpty(_currentTranscriptionId))
                {
                    _ = CancelTranscriptionOnServerAsync(_currentTranscriptionId);
                }
            }
        }
        #endregion

        #region Helper Methods
        private void OnTranscriptionProgressUpdate(TranscriptionProgressUpdate update)
        {
            TranscriptionProgress = (int)update.ProgressPercent;
            StatusMessage = update.Message ?? "Processing...";

            if (!string.IsNullOrEmpty(update.TranscriptionId))
            {
                _currentTranscriptionId = update.TranscriptionId;
            }

            // Update queue item if it exists
            var queueItem = QueueItems.FirstOrDefault(q => q.TranscriptionId == _currentTranscriptionId);
            if (queueItem != null)
            {
                queueItem.Progress = (int)update.ProgressPercent;
                queueItem.Status = GetStatusFromPhase(update.Phase);
                queueItem.LastUpdated = DateTime.Now;
            }
        }

        private async Task PollTranscriptionStatusAsync(string transcriptionId, TranscriptionQueueItem queueItem)
        {
            try
            {
                var cts = new CancellationTokenSource();
                var delay = TimeSpan.FromSeconds(10); // Start with 10 second polling interval

                while (true)
                {
                    await Task.Delay(delay, cts.Token);

                    var status = await _transcriptionService.GetTranscriptionStatusAsync(transcriptionId, cts.Token);
                    queueItem.Status = status;
                    queueItem.LastUpdated = DateTime.Now;

                    if (status == TranscriptionStatus.Completed)
                    {
                        // Download the completed transcription
                        var outputPath = System.IO.Path.Combine(
                            System.IO.Path.GetDirectoryName(queueItem.MediaPath) ?? string.Empty,
                            System.IO.Path.GetFileNameWithoutExtension(queueItem.MediaPath) + ".json");

                        var downloadProgress = new Progress<TranscriptionProgressUpdate>(update => 
                        {
                            queueItem.Progress = (int)update.ProgressPercent;
                        });

                        var success = await _transcriptionService.DownloadTranscriptionFileAsync(
                            transcriptionId, outputPath, downloadProgress, cts.Token);

                        if (success)
                        {
                            queueItem.OutputPath = outputPath;
                            queueItem.Progress = 100;
                            _logger.LogInformation("Downloaded transcription to {OutputPath}", outputPath);
                        }
                        break;
                    }
                    else if (status == TranscriptionStatus.Failed || status == TranscriptionStatus.Cancelled)
                    {
                        _logger.LogWarning("Transcription {TranscriptionId} ended with status {Status}", 
                            transcriptionId, status);
                        break;
                    }

                    // Adaptive polling - increase delay for long-running transcriptions
                    if (delay.TotalSeconds < 60)
                    {
                        delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 1.5, 60));
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error polling transcription status for {TranscriptionId}", transcriptionId);
                queueItem.Status = TranscriptionStatus.Failed;
                queueItem.LastUpdated = DateTime.Now;
            }
        }

        private async Task CancelTranscriptionOnServerAsync(string transcriptionId)
        {
            try
            {
                var success = await _transcriptionService.CancelTranscriptionAsync(transcriptionId, CancellationToken.None);
                if (success)
                {
                    _logger.LogInformation("Successfully canceled transcription {TranscriptionId} on server", transcriptionId);
                }
                else
                {
                    _logger.LogWarning("Failed to cancel transcription {TranscriptionId} on server", transcriptionId);
                }

                // Update queue item
                var queueItem = QueueItems.FirstOrDefault(q => q.TranscriptionId == transcriptionId);
                if (queueItem != null)
                {
                    queueItem.Status = TranscriptionStatus.Cancelled;
                    queueItem.LastUpdated = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling transcription {TranscriptionId} on server", transcriptionId);
            }
        }

        private TranscriptionStatus GetStatusFromPhase(TranscriptionPhase phase) => phase switch
        {
            TranscriptionPhase.Queued => TranscriptionStatus.Queued,
            TranscriptionPhase.SilenceDetection => TranscriptionStatus.Processing,
            TranscriptionPhase.AudioExtraction => TranscriptionStatus.Processing,
            TranscriptionPhase.Uploading => TranscriptionStatus.Processing,
            TranscriptionPhase.Submitted => TranscriptionStatus.Processing,
            TranscriptionPhase.Processing => TranscriptionStatus.Processing,
            TranscriptionPhase.Downloading => TranscriptionStatus.Processing,
            TranscriptionPhase.Completed => TranscriptionStatus.Completed,
            TranscriptionPhase.Failed => TranscriptionStatus.Failed,
            TranscriptionPhase.Cancelled => TranscriptionStatus.Cancelled,
            _ => TranscriptionStatus.Unknown
        };

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }

    /// <summary>
    /// Represents an item in the transcription queue
    /// </summary>
    public class TranscriptionQueueItem : INotifyPropertyChanged
    {
        private string _mediaPath = string.Empty;
        private string _transcriptionId = string.Empty;
        private string? _outputPath;
        private TranscriptionStatus _status;
        private int _progress;
        private DateTime _submittedAt;
        private DateTime _lastUpdated;

        /// <summary>
        /// Gets or sets the media file path
        /// </summary>
        public string MediaPath
        {
            get => _mediaPath;
            set
            {
                if (_mediaPath != value)
                {
                    _mediaPath = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the transcription ID
        /// </summary>
        public string TranscriptionId
        {
            get => _transcriptionId;
            set
            {
                if (_transcriptionId != value)
                {
                    _transcriptionId = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the output file path
        /// </summary>
        public string? OutputPath
        {
            get => _outputPath;
            set
            {
                if (_outputPath != value)
                {
                    _outputPath = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCompleted));
                }
            }
        }

        /// <summary>
        /// Gets or sets the transcription status
        /// </summary>
        public TranscriptionStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusText));
                    OnPropertyChanged(nameof(IsCompleted));
                    OnPropertyChanged(nameof(IsFailed));
                }
            }
        }

        /// <summary>
        /// Gets the status as a display-friendly string
        /// </summary>
        public string StatusText => Status switch
        {
            TranscriptionStatus.Queued => "Queued",
            TranscriptionStatus.Processing => "Processing",
            TranscriptionStatus.Completed => "Completed",
            TranscriptionStatus.Failed => "Failed",
            TranscriptionStatus.Cancelled => "Cancelled",
            _ => "Unknown"
        };

        /// <summary>
        /// Gets or sets the progress (0-100)
        /// </summary>
        public int Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets when the transcription was submitted
        /// </summary>
        public DateTime SubmittedAt
        {
            get => _submittedAt;
            set
            {
                if (_submittedAt != value)
                {
                    _submittedAt = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets when the transcription was last updated
        /// </summary>
        public DateTime LastUpdated
        {
            get => _lastUpdated;
            set
            {
                if (_lastUpdated != value)
                {
                    _lastUpdated = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets whether the transcription is completed
        /// </summary>
        public bool IsCompleted => Status == TranscriptionStatus.Completed && !string.IsNullOrEmpty(OutputPath);

        /// <summary>
        /// Gets whether the transcription failed
        /// </summary>
        public bool IsFailed => Status == TranscriptionStatus.Failed;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

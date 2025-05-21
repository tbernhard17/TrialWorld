using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models.Transcription;
using TrialWorld.Presentation.Commands;

namespace TrialWorld.Presentation.ViewModels
{
    /// <summary>
    /// ViewModel for the MediaTranscriptControl
    /// </summary>
    public class MediaTranscriptViewModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly ILogger<MediaTranscriptViewModel> _logger;
        private readonly ITranscriptionService _transcriptionService;
        private readonly CourtroomMediaPlayerViewModel _mediaPlayerViewModel = null!; // Initialized in constructor
        
        private string _mediaPath = string.Empty;
        private TimeSpan _currentPosition = TimeSpan.Zero;
        private bool _isProcessing;
        private string _statusMessage = "Ready";
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isSyncedWithMedia = true;
        private ObservableCollection<TranscriptSegment> _transcriptSegments = new ObservableCollection<TranscriptSegment>();

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
                    _logger?.LogInformation("Media path updated in transcript view: {Path}", value);
                    LoadTranscriptAsync().ConfigureAwait(false);
                    OnPropertyChanged(nameof(HasMedia));
                }
            }
        }

        /// <summary>
        /// Gets or sets the current media position
        /// </summary>
        public TimeSpan CurrentPosition
        {
            get => _currentPosition;
            set
            {
                if (SetProperty(ref _currentPosition, value))
                {
                    // Update the current segment in the transcript view
                    UpdateCurrentSegment();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the transcript is being processed
        /// </summary>
        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        /// <summary>
        /// Gets or sets the current status message
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the transcript is synced with the media player
        /// </summary>
        public bool IsSyncedWithMedia
        {
            get => _isSyncedWithMedia;
            set => SetProperty(ref _isSyncedWithMedia, value);
        }

        /// <summary>
        /// Gets the transcript segments collection
        /// </summary>
        public ObservableCollection<TranscriptSegment> TranscriptSegments => _transcriptSegments;

        /// <summary>
        /// Gets a value indicating whether a media file is loaded
        /// </summary>
        public bool HasMedia => !string.IsNullOrEmpty(_mediaPath) && File.Exists(_mediaPath);

        /// <summary>
        /// Gets a value indicating whether a transcript is available
        /// </summary>
        public bool HasTranscript => _transcriptSegments.Count > 0;

        /// <summary>
        /// Gets the command to transcribe the current media file
        /// </summary>
        public ICommand TranscribeCommand { get; }

        /// <summary>
        /// Gets the command to export the transcript
        /// </summary>
        public ICommand ExportCommand { get; }

        /// <summary>
        /// Gets the command to toggle auto-scrolling
        /// </summary>
        public ICommand ToggleAutoScrollCommand { get; }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Occurs when seeking to a specific position is requested
        /// </summary>
        public event EventHandler<TimeSpan>? SeekRequested;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the MediaTranscriptViewModel class
        /// </summary>
        /// <param name="transcriptionService">Transcription service</param>
        /// <param name="mediaPlayerViewModel">Media player view model</param>
        /// <param name="logger">Logger</param>
        public MediaTranscriptViewModel(
            ITranscriptionService transcriptionService,
            CourtroomMediaPlayerViewModel mediaPlayerViewModel,
            ILogger<MediaTranscriptViewModel> logger)
        {
            _transcriptionService = transcriptionService ?? throw new ArgumentNullException(nameof(transcriptionService));
            _mediaPlayerViewModel = mediaPlayerViewModel ?? throw new ArgumentNullException(nameof(mediaPlayerViewModel));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Initialize commands
            TranscribeCommand = new AsyncRelayCommand(TranscribeAsync, () => HasMedia && !IsProcessing);
            ExportCommand = new AsyncRelayCommand(ExportAsync, () => HasTranscript && !IsProcessing);
            ToggleAutoScrollCommand = new RelayCommand(ToggleAutoScroll);

            // Subscribe to media player events
            if (_mediaPlayerViewModel != null)
            {
                // Monitor media path changes
                _mediaPlayerViewModel.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(CourtroomMediaPlayerViewModel.CurrentMediaPath))
                    {
                        MediaPath = _mediaPlayerViewModel.CurrentMediaPath;
                    }
                };

                // Initialize with current media path if available
                if (!string.IsNullOrEmpty(_mediaPlayerViewModel.CurrentMediaPath))
                {
                    MediaPath = _mediaPlayerViewModel.CurrentMediaPath;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads transcript data for the current media
        /// </summary>
        public async Task LoadTranscriptAsync()
        {
            if (string.IsNullOrEmpty(_mediaPath) || !File.Exists(_mediaPath))
            {
                return;
            }

            try
            {
                IsProcessing = true;
                StatusMessage = "Loading transcript...";

                // Check if a transcript file exists for this media
                var directory = Path.GetDirectoryName(_mediaPath);
                var fileName = Path.GetFileNameWithoutExtension(_mediaPath);
                var transcriptPath = Path.Combine(directory ?? string.Empty, $"{fileName}.json");

                if (File.Exists(transcriptPath))
                {
                    // Load existing transcript
                    await LoadTranscriptFileAsync(transcriptPath);
                    StatusMessage = "Transcript loaded";
                }
                else
                {
                    // Clear existing segments
                    _transcriptSegments.Clear();
                    StatusMessage = "No transcript available";
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading transcript for media {MediaPath}", _mediaPath);
                StatusMessage = "Error loading transcript";
            }
            finally
            {
                IsProcessing = false;
                OnPropertyChanged(nameof(HasTranscript));
            }
        }

        /// <summary>
        /// Loads transcript data from a file
        /// </summary>
        /// <param name="filePath">Path to the transcript file</param>
        private async Task LoadTranscriptFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return;
            }

            try
            {
                // Clear existing segments
                _transcriptSegments.Clear();

                // Read and parse the transcript file
                string json = await File.ReadAllTextAsync(filePath);
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var result = System.Text.Json.JsonSerializer.Deserialize<TranscriptionResult>(json, options);

                if (result != null && result.Segments != null)
                {
                    // Add segments to the collection
                    foreach (var segment in result.Segments)
                    {
                        _transcriptSegments.Add(segment);
                    }

                    StatusMessage = $"Loaded {_transcriptSegments.Count} transcript segments";
                }
                else
                {
                    StatusMessage = "No transcript segments found";
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading transcript file: {FilePath}", filePath);
                StatusMessage = "Error loading transcript";
            }

            OnPropertyChanged(nameof(HasTranscript));
        }

        /// <summary>
        /// Transcribes the current media file
        /// </summary>
        private async Task TranscribeAsync()
        {
            if (!HasMedia)
            {
                MessageBox.Show("No media file selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                IsProcessing = true;
                StatusMessage = "Transcribing media...";

                // Cancel any previous operations
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();

                // Create transcription options
                var options = new TranscriptionOptions
                {
                    EnableSpeakerDiarization = true,
                    EnableWordTimestamps = true,
                    EnablePunctuation = true,
                    Language = "en"
                };

                // Start transcription
                var result = await _transcriptionService.TranscribeAsync(_mediaPath, _cancellationTokenSource.Token);

                // Process result
                if (result != null && result.Segments != null)
                {
                    // Clear existing segments
                    _transcriptSegments.Clear();

                    // Add segments to the collection
                    foreach (var segment in result.Segments)
                    {
                        _transcriptSegments.Add(segment);
                    }

                    StatusMessage = $"Transcription completed with {_transcriptSegments.Count} segments";
                }
                else
                {
                    StatusMessage = "Transcription completed with no segments";
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Transcription canceled";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error transcribing media: {Path}", _mediaPath);
                StatusMessage = "Error transcribing media";
                MessageBox.Show($"Error transcribing media: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
                OnPropertyChanged(nameof(HasTranscript));
            }
        }

        /// <summary>
        /// Exports the transcript to a file
        /// </summary>
        private async Task ExportAsync()
        {
            if (!HasTranscript)
            {
                MessageBox.Show("No transcript available to export.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Export Transcript",
                    Filter = "SRT Subtitles (*.srt)|*.srt|WebVTT Subtitles (*.vtt)|*.vtt|Text Files (*.txt)|*.txt|JSON Files (*.json)|*.json",
                    DefaultExt = ".srt",
                    FileName = Path.GetFileNameWithoutExtension(_mediaPath) + ".srt"
                };

                if (dialog.ShowDialog() == true)
                {
                    IsProcessing = true;
                    StatusMessage = "Exporting transcript...";

                    string ext = Path.GetExtension(dialog.FileName).ToLowerInvariant();

                    // Create TranscriptionResult from the current segments
                    var result = new TranscriptionResult
                    {
                        TranscriptPath = dialog.FileName,
                        DetectedLanguage = "en-US",
                        Success = true,
                        Segments = new List<TranscriptSegment>(_transcriptSegments)
                    };

                    // Export transcript based on file extension
                    switch (ext)
                    {
                        case ".srt":
                            await ExportToSrtAsync(result, dialog.FileName);
                            break;
                        case ".vtt":
                            await ExportToVttAsync(result, dialog.FileName);
                            break;
                        case ".txt":
                            await ExportToTextAsync(result, dialog.FileName);
                            break;
                        case ".json":
                            await ExportToJsonAsync(result, dialog.FileName);
                            break;
                        default:
                            throw new NotSupportedException($"Unsupported transcript format: {ext}");
                    }

                    StatusMessage = "Transcript exported";
                    MessageBox.Show($"Transcript exported to {dialog.FileName}", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting transcript");
                StatusMessage = "Error exporting transcript";
                MessageBox.Show($"Error exporting transcript: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Exports the transcript to SRT format
        /// </summary>
        private async Task ExportToSrtAsync(TranscriptionResult result, string filePath)
        {
            using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
            int index = 1;

            foreach (var segment in result.Segments)
            {
                // Format: index, timestamp range, text
                await writer.WriteLineAsync(index.ToString());
                
                // Format timestamps as HH:MM:SS,mmm --> HH:MM:SS,mmm
                TimeSpan startTime = TimeSpan.FromMilliseconds(segment.StartTime);
                TimeSpan endTime = TimeSpan.FromMilliseconds(segment.EndTime);
                
                await writer.WriteLineAsync($"{FormatSrtTimestamp(startTime)} --> {FormatSrtTimestamp(endTime)}");
                await writer.WriteLineAsync(segment.Text);
                await writer.WriteLineAsync(); // Empty line between entries
                
                index++;
            }
        }

        /// <summary>
        /// Exports the transcript to WebVTT format
        /// </summary>
        private async Task ExportToVttAsync(TranscriptionResult result, string filePath)
        {
            using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
            
            // WebVTT header
            await writer.WriteLineAsync("WEBVTT");
            await writer.WriteLineAsync();
            
            int index = 1;
            foreach (var segment in result.Segments)
            {
                // Format: optional cue identifier, timestamp range, text
                await writer.WriteLineAsync($"Cue {index}");
                
                // Format timestamps as HH:MM:SS.mmm --> HH:MM:SS.mmm
                TimeSpan startTime = TimeSpan.FromMilliseconds(segment.StartTime);
                TimeSpan endTime = TimeSpan.FromMilliseconds(segment.EndTime);
                
                await writer.WriteLineAsync($"{FormatVttTimestamp(startTime)} --> {FormatVttTimestamp(endTime)}");
                await writer.WriteLineAsync(segment.Text);
                await writer.WriteLineAsync(); // Empty line between entries
                
                index++;
            }
        }

        /// <summary>
        /// Exports the transcript to plain text format
        /// </summary>
        private async Task ExportToTextAsync(TranscriptionResult result, string filePath)
        {
            using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
            
            foreach (var segment in result.Segments)
            {
                // Format: [timestamp] Speaker: Text
                TimeSpan startTime = TimeSpan.FromMilliseconds(segment.StartTime);
                string speaker = !string.IsNullOrEmpty(segment.Speaker) ? $"{segment.Speaker}: " : "";
                
                await writer.WriteLineAsync($"[{FormatTextTimestamp(startTime)}] {speaker}{segment.Text}");
            }
        }

        /// <summary>
        /// Exports the transcript to JSON format
        /// </summary>
        private async Task ExportToJsonAsync(TranscriptionResult result, string filePath)
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            string json = System.Text.Json.JsonSerializer.Serialize(result, options);
            await File.WriteAllTextAsync(filePath, json);
        }

        /// <summary>
        /// Formats a timestamp for SRT format (HH:MM:SS,mmm)
        /// </summary>
        private string FormatSrtTimestamp(TimeSpan time)
        {
            return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2},{time.Milliseconds:D3}";
        }

        /// <summary>
        /// Formats a timestamp for WebVTT format (HH:MM:SS.mmm)
        /// </summary>
        private string FormatVttTimestamp(TimeSpan time)
        {
            return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}.{time.Milliseconds:D3}";
        }

        /// <summary>
        /// Formats a timestamp for text format (HH:MM:SS)
        /// </summary>
        private string FormatTextTimestamp(TimeSpan time)
        {
            return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
        }

        /// <summary>
        /// Updates the current segment based on the media position
        /// </summary>
        private void UpdateCurrentSegment()
        {
            if (!IsSyncedWithMedia || _transcriptSegments.Count == 0)
            {
                return;
            }

            // Find the current segment based on media position
            var currentSegment = _transcriptSegments.FirstOrDefault(s =>
                _currentPosition >= TimeSpan.FromMilliseconds(s.StartTime) && 
                _currentPosition <= TimeSpan.FromMilliseconds(s.EndTime));

            // If no segment found at current position, try to find the closest upcoming segment
            if (currentSegment == null)
            {
                currentSegment = _transcriptSegments
                    .Where(s => _currentPosition <= TimeSpan.FromMilliseconds(s.StartTime))
                    .OrderBy(s => s.StartTime)
                    .FirstOrDefault();
            }

            // If a segment is found, notify the UI to highlight it
            if (currentSegment != null)
            {
                // This would typically be handled by the UI through data binding
                _logger?.LogDebug("Current segment: {Text}", currentSegment.Text);
            }
        }

        /// <summary>
        /// Toggles auto-scrolling behavior
        /// </summary>
        private void ToggleAutoScroll()
        {
            IsSyncedWithMedia = !IsSyncedWithMedia;
            _logger?.LogInformation("Auto-scroll toggled to: {State}", IsSyncedWithMedia ? "On" : "Off");
        }

        /// <summary>
        /// Seeks to a specific position in the media
        /// </summary>
        /// <param name="position">Position to seek to</param>
        public void SeekToPosition(TimeSpan position)
        {
            SeekRequested?.Invoke(this, position);
        }

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">Name of the property that changed</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets a property value and raises the PropertyChanged event if the value changed
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="storage">Reference to the backing field</param>
        /// <param name="value">New value</param>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>True if the value changed, false otherwise</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}

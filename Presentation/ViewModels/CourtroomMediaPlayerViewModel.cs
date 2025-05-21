using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Interfaces.Services;
using TrialWorld.Core.Models;
using TrialWorld.Presentation.Commands;

namespace TrialWorld.Presentation.ViewModels
{
    public class CourtroomMediaPlayerViewModel : INotifyPropertyChanged
    {
        private readonly ILogger<CourtroomMediaPlayerViewModel> _logger; // Updated logger type
        private readonly ISilenceDetectionService _silenceDetectionService;
        private string _currentMediaPath = string.Empty;
        private bool _isProcessing;
        private int _processingProgress;
        private string _statusMessage = "Ready";
        private CancellationTokenSource? _cancellationTokenSource;
        private List<SilencePeriod> _silencePeriods = new List<SilencePeriod>();
        private bool _hasSilenceDetectionResults;
        private TimeSpan _currentPosition;

        /// <summary>
        /// Initializes a new instance of the CourtroomMediaPlayerViewModel class.
        /// </summary>
        /// <param name="silenceDetectionService">Service for silence detection and removal.</param> // Updated param name for clarity
        /// <param name="logger">Logger for recording operations.</param>
        public CourtroomMediaPlayerViewModel(
            ISilenceDetectionService silenceDetectionService,
            ILogger<CourtroomMediaPlayerViewModel> logger) // Updated logger type
        {
            _silenceDetectionService = silenceDetectionService ?? throw new ArgumentNullException(nameof(silenceDetectionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _statusMessage = "Ready";
            
            // Initialize commands
            DetectSilenceCommand = new AsyncRelayCommand(DetectSilenceAsync, () => CanDetectSilence);
            RemoveSilenceCommand = new AsyncRelayCommand(RemoveSilenceAsync, () => CanRemoveSilence);
            CancelProcessingCommand = new RelayCommand(CancelProcessing, () => CanCancelProcessing);
            SaveVideoAfterSilenceCommand = new AsyncRelayCommand(SaveVideoAfterSilenceAsync, CanSaveVideoAfterSilence);
        }
        
        /// <summary>
        /// Seeks to a specific position in the media in milliseconds
        /// </summary>
        /// <param name="positionMs">Position in milliseconds to seek to</param>
        public void SeekToPosition(int positionMs)
        {
            try
            {
                // Convert milliseconds to TimeSpan for media element
                var position = TimeSpan.FromMilliseconds(positionMs);
                CurrentPosition = position;
                
                // Raise property changed to notify UI
                OnPropertyChanged(nameof(CurrentPosition));
                
                _logger.LogInformation("Seeking to position: {Position}ms", positionMs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeking to position {Position}ms", positionMs);
            }
        }

        /// <summary>
        /// Determines whether the video can be saved with silence removed.
        /// </summary>
        private bool CanSaveVideoAfterSilence()
        {
            return HasSilenceDetectionResults && !IsProcessing;
        }

        /// <summary>
        /// Gets or sets a value indicating whether silence detection results are available.
        /// </summary>
        public bool HasSilenceDetectionResults
        {
            get => _hasSilenceDetectionResults || (_silencePeriods != null && _silencePeriods.Count > 0);
            set 
            { 
                _hasSilenceDetectionResults = value; 
                OnPropertyChanged();
                // Also update the command can-execute state
                (RemoveSilenceCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (SaveVideoAfterSilenceCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets the command to detect silence in the current media file.
        /// </summary>
        public ICommand DetectSilenceCommand { get; }
        
        /// <summary>
        /// Gets the command to remove silence from the current media file.
        /// </summary>
        public ICommand RemoveSilenceCommand { get; }
        
        /// <summary>
        /// Gets the command to cancel an ongoing processing operation.
        /// </summary>
        public ICommand CancelProcessingCommand { get; }
        
        /// <summary>
        /// Gets the command to save the video with detected silence removed.
        /// </summary>
        public ICommand SaveVideoAfterSilenceCommand { get; }
        
        /// <summary>
        /// Gets or sets the current media file path.
        /// </summary>
        public string CurrentMediaPath
        {
            get => _currentMediaPath;
            set 
            { 
                if (SetProperty(ref _currentMediaPath, value))
                {
                    _logger?.LogInformation("Media path updated: {Path}", value);
                    // Reset silence detection results when loading a new media file
                    if (_silencePeriods != null && _silencePeriods.Count > 0)
                    {
                        _silencePeriods.Clear();
                        HasSilenceDetectionResults = false;
                    }
                    
                    // Notify commands that they may need to update their can-execute state
                    (DetectSilenceCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                    (RemoveSilenceCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }
        
        /// <summary>
        /// Loads a media file for playback and processing.
        /// </summary>
        /// <param name="filePath">The path to the media file.</param>
        public void LoadMedia(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                _logger?.LogWarning("Attempted to load non-existent file: {FilePath}", filePath);
                return;
            }
            
            try
            {
                // Update the media path property
                CurrentMediaPath = filePath;
                StatusMessage = "Media loaded: " + Path.GetFileName(filePath);
                _logger?.LogInformation("Media file loaded successfully: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading media file: {FilePath}", filePath);
                StatusMessage = "Error loading media";
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether a processing operation is in progress.
        /// </summary>
        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                if (SetProperty(ref _isProcessing, value))
                {
                    // Update command availability
                    ((AsyncRelayCommand)DetectSilenceCommand).RaiseCanExecuteChanged();
                    ((AsyncRelayCommand)RemoveSilenceCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)CancelProcessingCommand).RaiseCanExecuteChanged();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the current processing progress percentage (0-100).
        /// </summary>
        public int ProcessingProgress
        {
            get => _processingProgress;
            set => SetProperty(ref _processingProgress, value);
        }
        
        /// <summary>
        /// Gets or sets the current playback position.
        /// </summary>
        public TimeSpan CurrentPosition
        {
            get => _currentPosition;
            set => SetProperty(ref _currentPosition, value);
        }

        /// <summary>
        /// Gets or sets the current status message.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }
        
        /// <summary>
        /// Gets a value indicating whether silence detection can be performed.
        /// </summary>
        public bool CanDetectSilence => !IsProcessing && !string.IsNullOrEmpty(CurrentMediaPath) && File.Exists(CurrentMediaPath);
        
        // HasSilenceDetectionResults property is now implemented above with a proper getter/setter
        
        /// <summary>
        /// Gets a value indicating whether silence removal can be performed.
        /// </summary>
        public bool CanRemoveSilence => !IsProcessing && !string.IsNullOrEmpty(CurrentMediaPath) && File.Exists(CurrentMediaPath);
        
        /// <summary>
        /// Gets a value indicating whether an ongoing processing operation can be canceled.
        /// </summary>
        public bool CanCancelProcessing => IsProcessing && _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested;
        
        /// <summary>
        /// Detects silence in the current media file.
        /// </summary>
        public async Task DetectSilenceAsync()
        {
            if (string.IsNullOrEmpty(CurrentMediaPath) || !File.Exists(CurrentMediaPath))
            {
                MessageBox.Show("No media file is currently loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            try
            {
                _logger.LogInformation("DetectSilenceAsync started for: {Path}", CurrentMediaPath);
                IsProcessing = true;
                StatusMessage = "Detecting silence...";
                ProcessingProgress = 0;
                
                // Create cancellation token source
                _cancellationTokenSource = new CancellationTokenSource();
                
                // Create progress reporter
                var progress = new Progress<int>(p => ProcessingProgress = p);
                
                // Detect silence directly using the silence detection service
                _silencePeriods = await _silenceDetectionService.DetectSilenceAsync(
                    CurrentMediaPath,
                    -30, // Default noise floor
                    1.0, // Default minimum silence duration
                    progress,
                    _cancellationTokenSource.Token);

                _logger.LogInformation("Silence detection service returned {Count} periods.", _silencePeriods?.Count ?? 0);

                // Always update detection results flag and notify property changes
                HasSilenceDetectionResults = _silencePeriods != null && _silencePeriods.Count > 0;
                _logger.LogInformation("HasSilenceDetectionResults set to: {Value}", HasSilenceDetectionResults);
                OnPropertyChanged(nameof(HasSilenceDetectionResults));
                StatusMessage = $"Found {_silencePeriods?.Count ?? 0} silence periods";

                // Force UI update for command states
                OnPropertyChanged(nameof(CanRemoveSilence));
                CommandManager.InvalidateRequerySuggested();

                // Force update of the UI to reflect the new state
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    CommandManager.InvalidateRequerySuggested();
                });
                
                if (_silencePeriods != null && _silencePeriods.Count > 0)
                {
                    var message = $"Found {_silencePeriods.Count} silence periods in the media file.\n\n";
                    
                    for (int i = 0; i < Math.Min(10, _silencePeriods.Count); i++)
                    {
                        var period = _silencePeriods[i];
                        message += $"Period {i+1}: {period.StartTime:mm\\:ss\\.fff} to {period.EndTime:mm\\:ss\\.fff} (Duration: {period.Duration:mm\\:ss\\.fff})\n";
                    }
                    
                    if (_silencePeriods.Count > 10)
                    {
                        message += $"... and {_silencePeriods.Count - 10} more.";
                    }
                    
                    MessageBox.Show(message, "Silence Detection Results", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No silence periods were detected in the media file.", "Silence Detection Results", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Silence detection canceled";
                _logger.LogInformation("Silence detection canceled for file: {FilePath}", CurrentMediaPath);
            }
            catch (Exception ex)
            {
                StatusMessage = "Error detecting silence";
                _logger.LogError(ex, "Error detecting silence in file: {FilePath}", CurrentMediaPath);
                MessageBox.Show($"An error occurred while detecting silence: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }
        
        /// <summary>
        /// Removes silence from the current media file.
        /// </summary>
        public async Task RemoveSilenceAsync()
        {
            if (string.IsNullOrEmpty(CurrentMediaPath) || !File.Exists(CurrentMediaPath))
            {
                MessageBox.Show("No media file is currently loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check if silence detection has been performed
            if (!HasSilenceDetectionResults || _silencePeriods == null || _silencePeriods.Count == 0)
            {
                var result = MessageBox.Show(
                    "No silence detection results found. Would you like to detect silence first?",
                    "Silence Detection Required",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await DetectSilenceAsync();
                    // If still no results after detection, return
                    if (!HasSilenceDetectionResults) return;
                }
                else
                {
                    return;
                }
            }
            
            // Ask for output file location
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Save Processed File",
                Filter = "Media Files|*.mp4;*.avi;*.mov;*.wmv;*.mp3;*.wav|All Files|*.*",
                FileName = Path.GetFileNameWithoutExtension(CurrentMediaPath) + "_no_silence" + Path.GetExtension(CurrentMediaPath)
            };
            
            if (saveFileDialog.ShowDialog() != true)
            {
                return;
            }
            
            string outputFilePath = saveFileDialog.FileName;
            
            try
            {
                IsProcessing = true;
                StatusMessage = "Removing silence...";
                ProcessingProgress = 0;
                
                // Create cancellation token source
                _cancellationTokenSource = new CancellationTokenSource();
                
                // Create progress reporter for double values
                var progress = new Progress<double>(p => ProcessingProgress = (int)p);
                
                // Remove silence directly using the silence detection service
                await _silenceDetectionService.RemoveSilenceAsync(
                    CurrentMediaPath,
                    outputFilePath,
                    -30, // Default noise floor
                    1.0, // Default minimum silence duration
                    progress,
                    _cancellationTokenSource.Token);
                
                StatusMessage = "Silence removal completed";
                
                var result = MessageBox.Show(
                    $"Silence has been removed and saved to:\n{outputFilePath}\n\nWould you like to load the processed file now?",
                    "Silence Removal Complete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);
                
                if (result == MessageBoxResult.Yes)
                {
                    // Load the processed file
                    CurrentMediaPath = outputFilePath;
                    // Notify that the media file has changed and should be loaded
                    OnPropertyChanged(nameof(CurrentMediaPath));
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Silence removal canceled";
                _logger.LogInformation("Silence removal canceled for file: {FilePath}", CurrentMediaPath);
            }
            catch (Exception ex)
            {
                StatusMessage = "Error removing silence";
                _logger.LogError(ex, "Error removing silence from file: {FilePath}", CurrentMediaPath);
                MessageBox.Show($"An error occurred while removing silence: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }
        
        /// <summary>
        /// Cancels the current processing operation.
        /// </summary>
        private void CancelProcessing()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                StatusMessage = "Canceling...";
            }
        }
        
        /// <summary>
        /// Saves the video with detected silence removed (for SaveVideoAfterSilenceCommand).
        /// </summary>
        public async Task SaveVideoAfterSilenceAsync()
        {
            if (!HasSilenceDetectionResults || _silencePeriods == null || _silencePeriods.Count == 0)
            {
                MessageBox.Show("No silence detection results available. Please run silence detection first.", "No Silence Detected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Title = "Save Video With Silence Removed",
                Filter = "Media Files|*.mp4;*.avi;*.mov;*.wmv;*.mp3;*.wav|All Files|*.*",
                FileName = Path.GetFileNameWithoutExtension(CurrentMediaPath) + "_no_silence" + Path.GetExtension(CurrentMediaPath)
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

            string outputFilePath = saveFileDialog.FileName;

            try
            {
                IsProcessing = true;
                StatusMessage = "Saving video without silence...";
                ProcessingProgress = 0;
                _cancellationTokenSource = new CancellationTokenSource();
                var progress = new Progress<double>(p => ProcessingProgress = (int)p);

                await _silenceDetectionService.RemoveSilenceAsync(
                    CurrentMediaPath,
                    outputFilePath,
                    -30, // Default noise floor
                    1.0, // Default minimum silence duration
                    progress,
                    _cancellationTokenSource.Token);

                StatusMessage = "Video saved without silence.";
                MessageBox.Show($"Video saved to: {outputFilePath}", "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Save video operation canceled.";
                _logger.LogInformation("Save video operation canceled for file: {FilePath}", CurrentMediaPath);
            }
            catch (Exception ex)
            {
                StatusMessage = "Error saving video without silence.";
                _logger.LogError(ex, "Error saving video without silence for file: {FilePath}", CurrentMediaPath);
                MessageBox.Show($"An error occurred while saving video: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Exports silence detection results to a CSV file.
        /// </summary>
        /// <param name="filePath">The path to save the CSV file.</param>
        public void ExportSilenceDetectionResults(string filePath)
        {
            if (!HasSilenceDetectionResults)
            {
                throw new InvalidOperationException("No silence detection results available to export.");
            }
            
            try
            {
                _logger.LogInformation("Exporting silence detection results to: {FilePath}", filePath);
                
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Write header
                    writer.WriteLine("Index,Start Time (s),End Time (s),Duration (s),Start Time (hh:mm:ss.fff),End Time (hh:mm:ss.fff),Duration (hh:mm:ss.fff)");
                    
                    // Write data
                    for (int i = 0; i < _silencePeriods.Count; i++)
                    {
                        var period = _silencePeriods[i];
                        // Format time values separately to avoid escape sequence issues
                        string startTimeFormatted = period.StartTime.ToString("hh:mm:ss.fff");
                        string endTimeFormatted = period.EndTime.ToString("hh:mm:ss.fff");
                        string durationFormatted = period.Duration.ToString("hh:mm:ss.fff");
                        
                        // Write CSV line with proper quoting
                        writer.WriteLine(
                            $"{i+1},{period.StartTime.TotalSeconds:F3},{period.EndTime.TotalSeconds:F3},{period.Duration.TotalSeconds:F3}," + 
                            $"\"{startTimeFormatted}\",\"{endTimeFormatted}\",\"{durationFormatted}\"");
                    }
                }
                
                _logger.LogInformation("Successfully exported {Count} silence periods to: {FilePath}", _silencePeriods.Count, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting silence detection results to: {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// Seeks to the specified position in the media.
        /// </summary>
        /// <param name="position">The position to seek to.</param>
        public void Seek(TimeSpan position)
        {
            // TODO: Implement the actual seek logic.
            // This might involve setting a CurrentPosition property that the View binds to,
            // or directly interacting with a media player service/control if the ViewModel has a reference.
            _logger.LogInformation("Seek called with position: {Position}", position);
            // For now, just a placeholder.
            // Example: CurrentPosition = position; (if CurrentPosition is a bindable property)
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
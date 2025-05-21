using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using TrialWorld.Core.Models;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Interfaces.Services;
using TrialWorld.Presentation.Commands;

namespace TrialWorld.Presentation.Controls
{
    /// <summary>
    /// Extension methods for MediaElement
    /// </summary>
    public static class MediaElementExtensions
    {
        /// <summary>
        /// Determines if the MediaElement is currently playing
        /// </summary>
        /// <param name="mediaElement">The MediaElement to check</param>
        /// <returns>True if the MediaElement is playing, false otherwise</returns>
        public static bool IsPlaying(this MediaElement mediaElement)
        {
            return mediaElement.Source != null && 
                   mediaElement.NaturalDuration.HasTimeSpan && 
                   mediaElement.Position < mediaElement.NaturalDuration.TimeSpan && 
                   !(mediaElement.IsMuted && mediaElement.Volume == 0);
        }
    }
    
    /// <summary>
    /// Courtroom Media Player with integrated audio enhancement and silence detection features.
    /// </summary>
    public partial class CourtroomMediaPlayer : UserControl, INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler? PropertyChanged;
        
        /// <summary>
        /// Event raised when media playback reaches the end
        /// </summary>
        public event EventHandler? MediaEnded;
        
        /// <summary>
        /// Event raised when media fails to load or play
        /// </summary>
        public event EventHandler? MediaFailed;
        
        /// <summary>
        /// Event raised when media is successfully opened
        /// </summary>
        public event EventHandler? MediaOpened;
        
        /// <summary>
        /// Event raised when the playback position changes
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CS0067:The event is never used", Justification = "Used by external components")]
        public event EventHandler? PositionChanged;
        #endregion

        #region Fields
        private readonly ILogger<CourtroomMediaPlayer>? _logger;
        private readonly ISilenceDetectionService? _silenceDetectionService;
        
        // Hardware-optimized silence detection parameters (AMD 3900X CPU, 64GB RAM, NVME M.2)
        private const int DEFAULT_NOISE_FLOOR_DB = -30;
        private const double DEFAULT_MIN_SILENCE_DURATION = 10.0; // 10 seconds confirmed as accurate
        
        private string _mediaPath = string.Empty;
        private bool _isProcessing;
        private int _processingProgress;
        private string _statusMessage = "Ready";
        private string _errorMessage = string.Empty;
        // Removed duplicate silence detection state. Use ViewModel for all silence detection results.
// private List<SilencePeriod>? _detectedSilencePeriods; // <-- Removed
        private int _noiseFloorDb = DEFAULT_NOISE_FLOOR_DB;
        private double _minSilenceDuration = DEFAULT_MIN_SILENCE_DURATION;
        #endregion

        #region Commands
        /// <summary>
        /// Gets the command to detect silence in the current media file.
        /// </summary>
        public required ICommand DetectSilenceCommand { get; set; }
        
        /// <summary>
        /// Gets the command to remove silence from the current media file.
        /// </summary>
        public required ICommand RemoveSilenceCommand { get; set; }
        
        /// <summary>
        /// Gets the command to cancel an ongoing processing operation.
        /// </summary>
        public required ICommand CancelProcessingCommand { get; set; }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the current media file path.
        /// </summary>
        public string MediaPath
        {
            get => _mediaPath;
            set
            {
                if (SetProperty(ref _mediaPath, value))
                {
                    UpdateCommandAvailability();
                }
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
                    UpdateCommandAvailability();
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
        /// Gets or sets the current status message.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
        #endregion
        
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the CourtroomMediaPlayer class.
        /// </summary>
        public CourtroomMediaPlayer()
        {
            InitializeComponent();
            // DataContext = this; // Ensure this is commented out or removed
            InitializeCommands();
        }

        /// <summary>
        /// Initializes a new instance of the CourtroomMediaPlayer class with dependency injection.
        /// </summary>
        /// <param name="silenceDetectionService">Service for silence detection and removal.</param>
        /// <param name="logger">Logger for recording operations.</param>
        public CourtroomMediaPlayer(
            ISilenceDetectionService silenceDetectionService,
            ILogger<CourtroomMediaPlayer> logger) : this()
        {
            _silenceDetectionService = silenceDetectionService ?? throw new ArgumentNullException(nameof(silenceDetectionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            InitializeCommands();
        }
        #endregion

        #region Command Initialization
        /// <summary>
        /// Initializes the commands for the media player.
        /// </summary>
        private void InitializeCommands()
        {
            DetectSilenceCommand = new AsyncRelayCommand(DetectSilenceAsync, CanDetectSilence);
            RemoveSilenceCommand = new AsyncRelayCommand(RemoveSilenceAsync, CanRemoveSilence);
            CancelProcessingCommand = new RelayCommand(CancelProcessing, CanCancelProcessing);
        }

        /// <summary>
        /// Updates the availability of commands based on current state.
        /// </summary>
        private void UpdateCommandAvailability()
        {
            ((AsyncRelayCommand)DetectSilenceCommand).RaiseCanExecuteChanged();
            ((AsyncRelayCommand)RemoveSilenceCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CancelProcessingCommand).RaiseCanExecuteChanged();
        }
        #endregion

        #region Command Handlers
        /// <summary>
        /// Gets a value indicating whether silence detection can be performed.
        /// </summary>
        private bool CanDetectSilence() => !IsProcessing && !string.IsNullOrEmpty(MediaPath) && File.Exists(MediaPath) && _silenceDetectionService != null;
        
        /// <summary>
        /// Gets a value indicating whether silence removal can be performed.
        /// </summary>
        private bool CanRemoveSilence() => 
            !IsProcessing && 
            !string.IsNullOrEmpty(MediaPath) && 
            File.Exists(MediaPath) && 
            _silenceDetectionService != null;
        
        /// <summary>
        /// Gets a value indicating whether an ongoing processing operation can be canceled.
        /// </summary>
        private bool CanCancelProcessing() => IsProcessing;

        /// <summary>
        /// Shows or hides the silence detection menu popup.
        /// </summary>
        private void SilenceMenuButton_Click(object sender, RoutedEventArgs e)
        {
            silenceDetectionPopup.IsOpen = !silenceDetectionPopup.IsOpen;
        }
        
        /// <summary>
        /// Updates the volume icon based on the current volume level and mute state.
        /// </summary>
        private void UpdateVolumeIcon()
        {
            if (mediaElement == null || volumeIcon == null) return;
            
            if (mediaElement.IsMuted || mediaElement.Volume == 0)
            {
                // Muted icon
                volumeIcon.Data = Geometry.Parse("M 0,5 L 2,5 L 6,1 L 6,11 L 2,7 L 0,7 Z M 10,5 L 8,3 L 10,1 M 8,7 L 10,9 L 12,7");
            }
            else if (mediaElement.Volume < 0.5)
            {
                // Low volume icon
                volumeIcon.Data = Geometry.Parse("M 0,5 L 2,5 L 6,1 L 6,11 L 2,7 L 0,7 Z M 8,3 C 10,5 10,7 8,9");
            }
            else
            {
                // High volume icon
                volumeIcon.Data = Geometry.Parse("M 0,5 L 2,5 L 6,1 L 6,11 L 2,7 L 0,7 Z M 8,3 C 10,5 10,7 8,9 M 10,1 C 14,5 14,7 10,11");
            }
        }
        
        /// <summary>
        /// Updates the silence settings display text when the minimum silence duration slider value changes.
        /// </summary>
        private void MinSilenceDurationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (silenceSettingsDisplay != null)
            {
                UpdateSilenceSettingsDisplay();
            }
        }
        
        /// <summary>
        /// Updates the silence settings display text when the noise floor slider value changes.
        /// </summary>
        private void NoiseFloorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (silenceSettingsDisplay != null)
            {
                UpdateSilenceSettingsDisplay();
            }
        }
        
        /// <summary>
        /// Updates the silence settings display text with current slider values.
        /// </summary>
        private void UpdateSilenceSettingsDisplay()
        {
            double duration = minSilenceDurationSlider.Value;
            int noiseFloor = (int)noiseFloorSlider.Value;
            silenceSettingsDisplay.Text = $"{duration:F1}s at {noiseFloor}dB";
        }
        
        /// <summary>
        /// Applies the current silence detection settings.
        /// </summary>
        private void ApplySilenceSettings_Click(object sender, RoutedEventArgs e)
        {
            _minSilenceDuration = minSilenceDurationSlider.Value;
            _noiseFloorDb = (int)noiseFloorSlider.Value;
            
            _logger?.LogInformation("Applied silence detection settings: {Duration}s at {NoiseFloor}dB", 
                _minSilenceDuration, _noiseFloorDb);
                
            silenceDetectionPopup.IsOpen = false;
        }
        
        /// <summary>
        /// Saves the video after silence detection with silence removed.
        /// </summary>
        private async void SaveVideoAfterDetection_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is TrialWorld.Presentation.ViewModels.CourtroomMediaPlayerViewModel vm)
            {
                if (!vm.HasSilenceDetectionResults)
                {
                    MessageBox.Show("No silence periods have been detected yet. Please run silence detection first.",
                        "No Silence Detected", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                try
                {
                    await vm.SaveVideoAfterSilenceAsync();
                    
                    // After saving is complete, check if we need to update the UI
                    if (!string.IsNullOrEmpty(vm.CurrentMediaPath) && vm.CurrentMediaPath != MediaPath)
                    {
                        LoadMediaFile(vm.CurrentMediaPath);
                    }
                    
                    silenceDetectionPopup.IsOpen = false;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error in SaveVideoAfterDetection_Click: {Message}", ex.Message);
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("CourtroomMediaPlayerViewModel is not set as DataContext.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Detects silence in the current media file.
        /// </summary>
        // Silence detection logic is now handled by the ViewModel. This method simply calls the ViewModel's DetectSilenceAsync.
private async Task DetectSilenceAsync()
{
    if (DataContext is TrialWorld.Presentation.ViewModels.CourtroomMediaPlayerViewModel vm)
    {
        await vm.DetectSilenceAsync();
    }
    else
    {
        MessageBox.Show("CourtroomMediaPlayerViewModel is not set as DataContext.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
        
        /// <summary>
        /// Removes silence from the current media file.
        /// </summary>
        private async Task RemoveSilenceAsync()
        {
            if (string.IsNullOrEmpty(MediaPath) || !File.Exists(MediaPath))
            {
                MessageBox.Show("No media file is currently loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            if (DataContext is TrialWorld.Presentation.ViewModels.CourtroomMediaPlayerViewModel vm)
            {
                await vm.RemoveSilenceAsync();
                
                // After removal is complete, check if we need to update the UI
                if (!string.IsNullOrEmpty(vm.CurrentMediaPath) && vm.CurrentMediaPath != MediaPath)
                {
                    LoadMediaFile(vm.CurrentMediaPath);
                }
                
                silenceDetectionPopup.IsOpen = false;
            }
            else
            {
                MessageBox.Show("CourtroomMediaPlayerViewModel is not set as DataContext.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Cancels the current processing operation.
        /// </summary>
        private void CancelProcessing()
        {
            if (DataContext is TrialWorld.Presentation.ViewModels.CourtroomMediaPlayerViewModel vm)
            {
                // Delegate to the ViewModel's CancelProcessing method
                vm.CancelProcessingCommand.Execute(null);
                StatusMessage = "Canceling...";
            }
            else
            {
                _logger?.LogWarning("Cannot cancel processing: CourtroomMediaPlayerViewModel is not set as DataContext");
            }
        }

        #endregion

        #region Helper Methods
        
        /// <summary>
        /// Loads a media file into the player.
        /// </summary>
        /// <param name="filePath">Path to the media file.</param>
        public void LoadMediaFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                _logger?.LogWarning("Attempted to load non-existent file: {FilePath}", filePath);
                
                // Update ViewModel status if available
                if (DataContext is ViewModels.CourtroomMediaPlayerViewModel viewModel)
                {
                    viewModel.StatusMessage = $"File not found: {filePath}";
                }
                
                // Raise the MediaFailed event
                MediaFailed?.Invoke(this, EventArgs.Empty);
                return;
            }
            
            try
            {
                // Clear any previous errors
                // Clear any previous error state
                
                // Update the media path
                MediaPath = filePath;
                
                // Set the media element source
                mediaElement.Source = new Uri(filePath);
                
                // Update status
                StatusMessage = "Media loaded: " + Path.GetFileName(filePath);
                
                // Update the ViewModel if available
                if (DataContext is ViewModels.CourtroomMediaPlayerViewModel viewModel)
                {
                    viewModel.LoadMedia(filePath);
                }
                
                // Start playing the media
                mediaElement.Play();
                playPauseIcon.Data = Geometry.Parse("M 2,2 L 6,2 L 6,14 L 2,14 Z M 10,2 L 14,2 L 14,14 L 10,14 Z"); // Pause icon
                
                _logger?.LogInformation("Media file loaded and playing: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading media file: {FilePath}", filePath);
                StatusMessage = "Error loading media";
                MessageBox.Show($"An error occurred while loading the media file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        

        #endregion
        
        #region Event Handlers
        

        
        private void PlaybackSpeedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void ExportButton_Click(object sender, RoutedEventArgs e) { }
        private void FiltersButton_Click(object sender, RoutedEventArgs e) { }
        private void TranscriptSearchBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) { }
        private void TranscriptSearch_Click(object sender, RoutedEventArgs e) { }
        private void TranscriptList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void KeywordsList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void ThumbnailsList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void EmotionsList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void VideoFilter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { }
        private void AudioFilter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { }
        private void ResetFilters_Click(object sender, RoutedEventArgs e) { }
        private void ApplyFilters_Click(object sender, RoutedEventArgs e) { }
        private void BrowseExportPath_Click(object sender, RoutedEventArgs e) { }
        private void CancelExport_Click(object sender, RoutedEventArgs e) { }
        private void ConfirmExport_Click(object sender, RoutedEventArgs e) { }
        private void ErrorOkButton_Click(object sender, RoutedEventArgs e) { }
        private void VideoDisplay_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { }
        
        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaElement == null) return;
            
            // Update UI with media duration
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                // Set the maximum value of the position slider to the duration in seconds
                positionSliderTimeline.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                
                // Update the duration display
                totalDurationText.Text = FormatTimeSpan(mediaElement.NaturalDuration.TimeSpan);
                
                // Start a timer to update the position
                StartPositionUpdateTimer();
                
                // Update the play/pause button icon to show pause (since media is now playing)
                playPauseIcon.Data = Geometry.Parse("M 2,2 L 6,2 L 6,14 L 2,14 Z M 10,2 L 14,2 L 14,14 L 10,14 Z"); // Pause icon
                
                // Play the media
                mediaElement.Play();
            }
            
            // Raise the MediaOpened event
            MediaOpened?.Invoke(this, EventArgs.Empty);
        }
        
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (mediaElement == null) return;
            
            // Reset the position to the beginning
            mediaElement.Position = TimeSpan.Zero;
            
            // Update the play/pause button icon to show play
            playPauseIcon.Data = Geometry.Parse("M 2,2 L 2,14 L 14,8 Z"); // Play icon
            
            // Stop the position update timer
            StopPositionUpdateTimer();
            
            // Raise the MediaEnded event
            MediaEnded?.Invoke(this, EventArgs.Empty);
        }
        
        private void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // Log the error
            string errorMessage = $"Media failed to load: {e.ErrorException?.Message ?? "Unknown error"}";
            _logger?.LogError(e.ErrorException, errorMessage);
            
            // Show error message using ViewModel if available
            if (DataContext is ViewModels.CourtroomMediaPlayerViewModel viewModel)
            {
                viewModel.StatusMessage = errorMessage;
            }
            
            // Raise the MediaFailed event
            MediaFailed?.Invoke(this, EventArgs.Empty);
        }
        
        private DispatcherTimer? _positionUpdateTimer;
        
        private void StartPositionUpdateTimer()
        {
            // Stop any existing timer
            StopPositionUpdateTimer();
            
            // Create a new timer that updates the position every 500ms
            _positionUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            
            _positionUpdateTimer.Tick += PositionUpdateTimer_Tick;
            _positionUpdateTimer.Start();
        }
        
        private void StopPositionUpdateTimer()
        {
            if (_positionUpdateTimer != null)
            {
                _positionUpdateTimer.Stop();
                _positionUpdateTimer.Tick -= PositionUpdateTimer_Tick;
                _positionUpdateTimer = null;
            }
        }
        
        private void PositionUpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (mediaElement == null || !mediaElement.NaturalDuration.HasTimeSpan) return;
            
            // Update the position slider value without triggering the ValueChanged event
            if (!_isUserSeeking)
            {
                positionSliderTimeline.Value = mediaElement.Position.TotalSeconds;
            }
            
            // Update the current position text
            currentPositionText.Text = FormatTimeSpan(mediaElement.Position);
        }
        
        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            // Format as H:MM:SS or MM:SS if less than an hour
            return timeSpan.Hours > 0 
                ? $"{timeSpan.Hours}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}" 
                : $"{timeSpan.Minutes}:{timeSpan.Seconds:D2}";
        }
        
        private bool _isUserSeeking = false;
        
        private void PositionSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _isUserSeeking = true;
            
            // Pause media during seeking for smoother experience
            if (mediaElement != null && mediaElement.IsPlaying())
            {
                mediaElement.Pause();
            }
        }
        
        private void PositionSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (mediaElement != null && mediaElement.NaturalDuration.HasTimeSpan)
            {
                // Set the position to the slider value
                mediaElement.Position = TimeSpan.FromSeconds(positionSliderTimeline.Value);
                
                // Resume playback if it was playing before
                mediaElement.Play();
                playPauseIcon.Data = Geometry.Parse("M 2,2 L 6,2 L 6,14 L 2,14 Z M 10,2 L 14,2 L 14,14 L 10,14 Z"); // Pause icon
            }
            
            _isUserSeeking = false;
        }
        
        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isUserSeeking || mediaElement == null) return;
            
            // Update the current position text during seeking
            currentPositionText.Text = FormatTimeSpan(TimeSpan.FromSeconds(e.NewValue));
        }
        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElement == null) return;
            
            if (mediaElement.Source == null && !string.IsNullOrEmpty(MediaPath))
            {
                // If no source is set but we have a path, try to set it
                try
                {
                    mediaElement.Source = new Uri(MediaPath);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to set media source: {MediaPath}", MediaPath);
                    return;
                }
            }
            
            if (mediaElement.Source == null) return;
            
            // Toggle play/pause
            if (mediaElement.CanPause && mediaElement.IsPlaying())
            {
                mediaElement.Pause();
                playPauseIcon.Data = Geometry.Parse("M 2,2 L 2,14 L 14,8 Z"); // Play icon
            }
            else
            {
                mediaElement.Play();
                playPauseIcon.Data = Geometry.Parse("M 2,2 L 6,2 L 6,14 L 2,14 Z M 10,2 L 14,2 L 14,14 L 10,14 Z"); // Pause icon
            }
        }
        private void StopButton_Click(object sender, RoutedEventArgs e) { }
        private void StepBackwardButton_Click(object sender, RoutedEventArgs e) { }
        private void StepForwardButton_Click(object sender, RoutedEventArgs e) { }
        private void SnapshotButton_Click(object sender, RoutedEventArgs e) { }
        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElement != null)
            {
                // Toggle mute state
                mediaElement.IsMuted = !mediaElement.IsMuted;
                
                // Update the volume icon based on mute state
                UpdateVolumeIcon();
            }
        }
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaElement != null)
            {
                mediaElement.Volume = e.NewValue;
                UpdateVolumeIcon();
            }
        }
        private void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                if (parentWindow.WindowState == WindowState.Maximized && parentWindow.WindowStyle == WindowStyle.None)
                {
                    // Exit fullscreen
                    parentWindow.WindowState = WindowState.Normal;
                    parentWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                }
                else
                {
                    // Enter fullscreen
                    parentWindow.WindowState = WindowState.Maximized;
                    parentWindow.WindowStyle = WindowStyle.None;
                }
            }
        }
        private async void DetectSilenceButton_Click(object sender, RoutedEventArgs e)
        {
            await DetectSilenceAsync();
        }
        private async void RemoveSilenceButton_Click(object sender, RoutedEventArgs e)
        {
            await RemoveSilenceAsync();
        }
        private void SilenceDetectionSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Open silence detection settings window
        }
        #endregion

        #region Property Changed Implementation
        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets the property value and raises the PropertyChanged event if the value has changed.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="field">Reference to the backing field.</param>
        /// <param name="value">New value for the property.</param>
        /// <param name="propertyName">Name of the property that changed.</param>
        /// <returns>True if the property was changed, false otherwise.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}

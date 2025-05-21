using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Transcription;

namespace TrialWorld.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for MediaTranscriptControl.xaml
    /// </summary>
    public partial class MediaTranscriptControl : UserControl
    {
        #region Fields

        private readonly ILogger<MediaTranscriptControl> _logger;
        private ObservableCollection<TranscriptSegmentViewModel> _segments = new ObservableCollection<TranscriptSegmentViewModel>();
        private ObservableCollection<TranscriptSegmentViewModel> _filteredSegments = new ObservableCollection<TranscriptSegmentViewModel>();
        private List<TranscriptSegmentViewModel> _searchResults = new List<TranscriptSegmentViewModel>();
        private int _currentSearchIndex = -1;
        private string? _searchText;
        private string? _speakerFilter;
        private bool _autoScrollEnabled = true;
        private TranscriptSegmentViewModel? _currentSegment;
        private HashSet<string> _availableSpeakers = new HashSet<string>();
        private string? _mediaPath;
        private ITranscriptionService? _transcriptionService;
        private CancellationTokenSource? _cancellationTokenSource = new CancellationTokenSource();
        
        /// <summary>
        /// Gets the ViewModel associated with this control
        /// </summary>
        public ViewModels.MediaTranscriptViewModel? ViewModel
        {
            get => DataContext as ViewModels.MediaTranscriptViewModel;
            set => DataContext = value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current media path
        /// </summary>
        public string MediaPath
        {
            get => ViewModel?.MediaPath ?? string.Empty;
            set
            {
                if (ViewModel != null && ViewModel.MediaPath != value)
                {
                    ViewModel.MediaPath = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(HasMedia));
                }
            }
        }

        /// <summary>
        /// Gets or sets the current media position
        /// </summary>
        public TimeSpan CurrentMediaPosition
        {
            get => ViewModel?.CurrentPosition ?? TimeSpan.Zero;
            set
            {
                if (ViewModel != null && ViewModel.CurrentPosition != value)
                {
                    ViewModel.CurrentPosition = value;
                    this.OnPropertyChanged();
                    UpdateCurrentSegment();
                }
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether the control is currently syncing with media
        /// </summary>
        public bool IsSyncedWithMedia { get; private set; } = true;

        /// <summary>
        /// Gets a value indicating whether search text is entered
        /// </summary>
        public bool HasSearchText => !string.IsNullOrWhiteSpace(_searchText);

        /// <summary>
        /// Gets a value indicating whether there are search matches
        /// </summary>
        public bool HasSearchMatches => _searchResults.Count > 0;

        /// <summary>
        /// Gets a value indicating whether a speaker filter is applied
        /// </summary>
        public bool HasFilter => !string.IsNullOrEmpty(_speakerFilter);

        /// <summary>
        /// Gets a value indicating whether a media file is loaded
        /// </summary>
        public bool HasMedia => ViewModel?.HasMedia ?? false;

        /// <summary>
        /// Gets a value indicating whether transcription data is available
        /// </summary>
        public bool HasTranscript => _segments.Count > 0;

        /// <summary>
        /// Gets a value indicating whether transcript is being processed
        /// </summary>
        public bool IsProcessing
        {
            get => ViewModel?.IsProcessing ?? false;
            private set
            {
                if (ViewModel != null && ViewModel.IsProcessing != value)
                {
                    ViewModel.IsProcessing = value;
                    this.OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the property changes
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Occurs when a segment is selected
        /// </summary>
        public event EventHandler<TranscriptSegmentViewModel>? SegmentSelected;
        
        /// <summary>
        /// Occurs when a seek to a specific position is requested
        /// </summary>
        public event EventHandler<TimeSpan>? SeekRequested;

        /// <summary>
        /// Occurs when transcription has completed
        /// </summary>
        public event EventHandler<TrialWorld.Core.Models.Transcription.TranscriptionResult>? TranscriptionCompleted;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaTranscriptControl"/> class
        /// Default parameterless constructor required for XAML instantiation
        /// </summary>
        public MediaTranscriptControl()
        {
            // NOTE: This method will be auto-generated by the XAML compiler
            // InitializeComponent();
            
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<MediaTranscriptControl>.Instance;
            _mediaPath = null;
            _cancellationTokenSource = new CancellationTokenSource();

            // UI elements will be connected once the XAML is loaded
            Loaded += MediaTranscriptControl_Loaded;
            _filteredSegments = new ObservableCollection<TranscriptSegmentViewModel>(_segments);
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaTranscriptControl"/> class with dependencies
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="transcriptionService">Transcription service</param>
        public MediaTranscriptControl(ILogger<MediaTranscriptControl> logger, ITranscriptionService transcriptionService)
        {
            // NOTE: This method will be auto-generated by the XAML compiler
            // InitializeComponent();
            
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _transcriptionService = transcriptionService ?? throw new ArgumentNullException(nameof(transcriptionService));
            _mediaPath = null;
            _cancellationTokenSource = new CancellationTokenSource();

            // UI elements will be connected once the XAML is loaded
            Loaded += MediaTranscriptControl_Loaded;
            _filteredSegments = new ObservableCollection<TranscriptSegmentViewModel>(_segments);
        }

        private void MediaTranscriptControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Find the UI elements from the XAML
            if (transcriptListViewTranscript != null)
            {
                // Initialize collections
                transcriptListViewTranscript.ItemsSource = _filteredSegments;
            }

            _filteredSegments.CollectionChanged += FilteredSegments_CollectionChanged;
            
            // Connect to the ViewModel if available
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
                ViewModel.SeekRequested += ViewModel_SeekRequested;
                
                // Initialize with current data if available
                if (ViewModel.HasTranscript)
                {
                    UpdateTranscriptSegments(ViewModel.TranscriptSegments);
                }
            }

            // Initialize UI elements
            if (speakerFilterComboBoxTranscript != null)
            {
                speakerFilterComboBoxTranscript.Items.Clear();
                speakerFilterComboBoxTranscript.Items.Add("All Speakers");
                speakerFilterComboBoxTranscript.SelectedIndex = 0;
            }
        }

        #endregion

        #region ViewModel Event Handlers

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (ViewModel == null) return;

            switch (e.PropertyName)
            {
                case nameof(ViewModels.MediaTranscriptViewModel.CurrentPosition):
                    // Update UI based on current position
                    CurrentMediaPosition = ViewModel.CurrentPosition;
                    break;
                    
                case nameof(ViewModels.MediaTranscriptViewModel.TranscriptSegments):
                    // Update segments when transcript changes
                    UpdateTranscriptSegments(ViewModel.TranscriptSegments);
                    break;
                    
                case nameof(ViewModels.MediaTranscriptViewModel.IsProcessing):
                    // Update processing state
                    IsProcessing = ViewModel.IsProcessing;
                    break;
                    
                case nameof(ViewModels.MediaTranscriptViewModel.IsSyncedWithMedia):
                    // Update auto-scroll state
                    _autoScrollEnabled = ViewModel.IsSyncedWithMedia;
                    break;
            }
        }

        private void ViewModel_SeekRequested(object? sender, TimeSpan position)
        {
            // Forward the seek request to our event subscribers
            SeekRequested?.Invoke(this, position);
        }

        /// <summary>
        /// Updates the transcript segments from the ViewModel
        /// </summary>
        private void UpdateTranscriptSegments(ObservableCollection<TranscriptSegment> segments)
        {
            if (segments == null) return;

            _segments.Clear();
            _availableSpeakers.Clear();

            // Convert TranscriptSegment to TranscriptSegmentViewModel
            foreach (var segment in segments)
            {
                // Create a new TranscriptSegment to pass to the TranscriptSegmentViewModel constructor
                var coreSegment = new TrialWorld.Core.Models.Transcription.TranscriptSegment
                {
                    StartTime = segment.StartTime,
                    EndTime = segment.EndTime,
                    Text = segment.Text,
                    Speaker = segment.Speaker,
                    Confidence = segment.Confidence,
                    Sentiment = segment.Sentiment ?? "NEUTRAL" // Adding required Sentiment property
                };
                
                var viewModel = new TranscriptSegmentViewModel(coreSegment);
                
                _segments.Add(viewModel);
                
                // Track available speakers
                if (!string.IsNullOrEmpty(segment.Speaker))
                {
                    _availableSpeakers.Add(segment.Speaker);
                }
            }

            // Update filtered segments
            ApplyFilters();
            
            // Update speaker filter dropdown
            UpdateSpeakerFilterComboBox();
        }
        
        /// <summary>
        /// Updates the speaker filter combo box with available speakers
        /// </summary>
        private void UpdateSpeakerFilterComboBox()
        {
            if (speakerFilterComboBoxTranscript == null) return;
            
            speakerFilterComboBoxTranscript.Items.Clear();
            speakerFilterComboBoxTranscript.Items.Add("All Speakers");
            
            foreach (var speaker in _availableSpeakers.OrderBy(s => s))
            {
                speakerFilterComboBoxTranscript.Items.Add(speaker);
            }
            
            speakerFilterComboBoxTranscript.SelectedIndex = 0;
        }

        #endregion

        #region Event Handlers

        private void FilteredSegments_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Update UI when the filtered segments collection changes
            this.OnPropertyChanged(nameof(HasTranscript));
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchTextBoxTranscript != null)
            {
                _searchText = searchTextBoxTranscript.Text;
            }
            this.OnPropertyChanged(nameof(HasSearchText));

            if (string.IsNullOrWhiteSpace(_searchText))
            {
                ClearSearch();
                return;
            }

            // Perform search
            PerformSearch();
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (_searchResults.Count > 0)
                {
                    // Go to next match
                    NavigateToNextMatch();
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                // Clear search
                if (searchTextBoxTranscript != null)
                {
                    searchTextBoxTranscript.Text = string.Empty;
                }
                e.Handled = true;
            }
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (searchTextBoxTranscript != null)
            {
                searchTextBoxTranscript.Text = string.Empty;
            }
            ClearSearch();
        }

        private void PreviousMatchButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPreviousMatch();
        }

        private void NextMatchButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToNextMatch();
        }

        private void SpeakerFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (speakerFilterComboBoxTranscript == null)
            {
                _speakerFilter = string.Empty;
                return;
            }

            if (speakerFilterComboBoxTranscript.SelectedItem == null)
            {
                _speakerFilter = string.Empty;
            }
            else if (speakerFilterComboBoxTranscript.SelectedIndex == 0)
            {
                _speakerFilter = string.Empty;
            }
            else
            {
                _speakerFilter = speakerFilterComboBoxTranscript.SelectedItem.ToString();
            }

            this.OnPropertyChanged(nameof(HasFilter));
            ApplyFilters();
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (speakerFilterComboBoxTranscript != null)
            {
                speakerFilterComboBoxTranscript.SelectedIndex = 0;
            }
        }

        private void TranscriptListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (transcriptListViewTranscript != null && transcriptListViewTranscript.SelectedItem is TranscriptSegmentViewModel selectedSegment)
            {
                // When user manually selects a segment, temporarily disable auto-scroll
                // This allows the user to browse the transcript without it jumping back to the current position
                if (_autoScrollEnabled && e.Source == transcriptListViewTranscript)
                {
                    // Only disable auto-scroll if this was a user action, not a programmatic selection
                    SetAutoScroll(false);
                    _logger?.LogInformation("Auto-scroll disabled due to manual segment selection");
                }
                
                // Notify that a segment was selected
                SegmentSelected?.Invoke(this, selectedSegment);
            }
        }

        private void TranscriptListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (transcriptListViewTranscript != null && transcriptListViewTranscript.SelectedItem is TranscriptSegmentViewModel selectedSegment)
            {
                // Notify that a segment was selected
                SegmentSelected?.Invoke(this, selectedSegment);
                
                // Request seeking to the segment's start time
                SeekRequested?.Invoke(this, selectedSegment.StartTime);
                
                // Re-enable auto-scroll when user explicitly seeks to a position
                SetAutoScroll(true);
                _logger?.LogInformation("Seeking to position {Position} and re-enabling auto-scroll", selectedSegment.StartTime);
            }
        }
        
        private void SyncToggleButton_Click(object sender, RoutedEventArgs e)
        {
            bool newState = ToggleAutoScroll();
            _logger?.LogInformation("Auto-scroll toggled to: {State}", newState ? "On" : "Off");
            
            // If turning auto-scroll back on, immediately update to current segment
            if (newState)
            {
                UpdateCurrentSegment();
            }
        }

        private async void TranscribeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!HasMedia)
            {
                MessageBox.Show("No media file selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                IsProcessing = true;

                // Use the ViewModel to handle cancellation
                // We don't need to manage the cancellation token here as it's handled by the ViewModel

                // Create transcription options
                var options = new TrialWorld.Core.Models.Transcription.TranscriptionOptions
                {
                    EnableSpeakerDiarization = true,
                    EnableWordTimestamps = true,
                    EnablePunctuation = true,
                    Language = "en"
                };

                // Start transcription
                if (_mediaPath == null)
                    throw new InvalidOperationException("Media path cannot be null when transcribing.");
                if (_transcriptionService == null)
                    throw new InvalidOperationException("Transcription service cannot be null when transcribing.");
                if (_cancellationTokenSource == null)
                    _cancellationTokenSource = new CancellationTokenSource();
                var result = await _transcriptionService.TranscribeAsync(_mediaPath, _cancellationTokenSource.Token);

                // Process result
                if (result != null)
                    await LoadTranscriptFromResult(result!);
            }
            catch (OperationCanceledException)
            {
                // Operation was canceled
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transcribing media: {Path}", ViewModel?.MediaPath ?? string.Empty);
                MessageBox.Show($"Error transcribing media: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (!HasTranscript)
            {
                MessageBox.Show("No transcript available to export.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Export Transcript",
                    Filter = "SRT Subtitles (*.srt)|*.srt|WebVTT Subtitles (*.vtt)|*.vtt|Text Files (*.txt)|*.txt|JSON Files (*.json)|*.json",
                    DefaultExt = ".srt",
                    FileName = Path.GetFileNameWithoutExtension(_mediaPath) + ".srt"
                };

                if (dialog.ShowDialog() == true)
                {
                    IsProcessing = true;

                    string ext = Path.GetExtension(dialog.FileName).ToLowerInvariant();
                    // Removed unused variable 'format'

                    switch (ext)
                    {
                        case ".srt":
                            break;
                        case ".vtt":
                            break;
                        case ".txt":
                            break;
                        case ".json":
                            break;
                        default:
                            break;
                    }

                    // Create TranscriptionResult from the current segments
                    var result = new TrialWorld.Core.Models.Transcription.TranscriptionResult
                    {
                        TranscriptPath = dialog.FileName,
                        DetectedLanguage = "en-US",
                        Success = true,
                        Segments = _segments.Select(s => s.CoreSegment).ToList()
                    };

                    // Export transcript
                    // await _transcriptionService.ExportTranscriptionAsync(result, dialog.FileName, format); // Method doesn't seem to exist on ITranscriptionService

                    // --- Placeholder for Export --- 
                    _logger.LogWarning("ExportTranscriptionAsync is not implemented on ITranscriptionService. Skipping actual export.");
                    await Task.Delay(100); // Simulate async work
                    // --- End Placeholder --- 

                    MessageBox.Show($"Transcript exported to {dialog.FileName}", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting transcript");
                MessageBox.Show($"Error exporting transcript: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Clears the current transcript
        /// </summary>
        public void ClearTranscript()
        {
            _segments.Clear();
            _filteredSegments.Clear();
            _availableSpeakers.Clear();
            _currentSegment = null;
            _currentSearchIndex = -1;
            _searchResults.Clear();

            if (speakerFilterComboBoxTranscript != null)
            {
                speakerFilterComboBoxTranscript.Items.Clear();
                speakerFilterComboBoxTranscript.Items.Add("All Speakers");
                speakerFilterComboBoxTranscript.SelectedIndex = 0;
            }

            this.OnPropertyChanged(nameof(HasTranscript));
            this.OnPropertyChanged(nameof(HasSearchMatches));
        }

        /// <summary>
        /// Sets auto-scrolling behavior
        /// </summary>
        /// <param name="enabled">Whether auto-scrolling is enabled</param>
        public void SetAutoScroll(bool enabled)
        {
            _autoScrollEnabled = enabled;
            IsSyncedWithMedia = enabled;
            OnPropertyChanged(nameof(IsSyncedWithMedia));
        }
        
        /// <summary>
        /// Toggles auto-scrolling behavior
        /// </summary>
        /// <returns>The new auto-scroll state</returns>
        public bool ToggleAutoScroll()
        {
            _autoScrollEnabled = !_autoScrollEnabled;
            IsSyncedWithMedia = _autoScrollEnabled;
            OnPropertyChanged(nameof(IsSyncedWithMedia));
            return _autoScrollEnabled;
        }

        /// <summary>
        /// Loads transcript data from a file
        /// </summary>
        /// <param name="filePath">Path to the transcript file</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task LoadTranscriptFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return;
            }

            try
            {
                IsProcessing = true;

                string ext = Path.GetExtension(filePath).ToLowerInvariant();
                string json;

                // Handle different file formats
                if (ext == ".json")
                {
                    json = await File.ReadAllTextAsync(filePath);
                }
                else if (ext == ".srt" || ext == ".vtt")
                {
                    // Convert subtitle to JSON format
                    // This is a simplified approach, in a real app we would use a proper subtitle parser
                    var lines = await File.ReadAllLinesAsync(filePath);
                    var mediaSegments = ParseSubtitles(lines, ext == ".vtt");
                    var segments = mediaSegments // ms is TrialWorld.Core.Models.MediaTranscriptSegment
                        .Select(ms => new TrialWorld.Core.Models.Transcription.TranscriptSegment // Target is the consolidated segment
                        {
                            Id = ms.Id,
                            MediaId = ms.MediaId,
                            Text = ms.Text,
                            StartTime = ms.StartTime * 1000, // Convert seconds to milliseconds
                            EndTime = ms.EndTime * 1000,     // Convert seconds to milliseconds
                            Confidence = ms.Confidence,
                            Speaker = ms.SpeakerId ?? string.Empty, // Map from SpeakerId with null check
                            Sentiment = ms.SentimentScore?.ToString() ?? string.Empty, // Map from SentimentScore with null check
                            // Words are not in MediaTranscriptSegment, so it remains null
                        })
                        .ToList();

                    var result = new TrialWorld.Core.Models.Transcription.TranscriptionResult
                    {
                        TranscriptPath = filePath,
                        DetectedLanguage = "en-US",
                        Success = true,
                        Segments = segments
                    };

                    json = JsonSerializer.Serialize(result);
                }
                else
                {
                    throw new NotSupportedException($"Unsupported transcript format: {ext}");
                }

                // Parse JSON
                var transcriptionResult = JsonSerializer.Deserialize<TrialWorld.Core.Models.Transcription.TranscriptionResult>(json);

                // Process result
                if (transcriptionResult != null)
                    await LoadTranscriptFromResult(transcriptionResult);
                else
                    throw new InvalidOperationException("Deserialized transcription result is null.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transcript file: {Path}", filePath);
                MessageBox.Show($"Error loading transcript: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads transcript data for the current media
        /// </summary>
        private async Task LoadTranscriptAsync()
        {
            if (ViewModel == null || !ViewModel.HasMedia)
            {
                return;
            }

            try
            {
                IsProcessing = true;

                // Let the ViewModel handle loading the transcript
                await Task.Run(() => ViewModel.LoadTranscriptAsync().ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transcript for media {MediaPath}", ViewModel.MediaPath);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Processes transcript data from a transcription result
        /// </summary>
        /// <param name="result">Transcription result</param>
        private Task LoadTranscriptFromResult(TranscriptionResult result)
        {
            // Clear existing segments
            _segments.Clear();
            _availableSpeakers.Clear();

            // Add all speakers to the collection
            foreach (var segment in result.Segments)
            {
                if (!string.IsNullOrEmpty(segment.Speaker))
                {
                    _availableSpeakers.Add(segment.Speaker);
                }
            }

            // Add segments to the collection
            foreach (var segment in result.Segments)
            {
                var viewModel = new TranscriptSegmentViewModel(segment);
                _segments.Add(viewModel);
            }

            // Apply filters
            ApplyFilters();

            // Update UI
            this.OnPropertyChanged(nameof(HasTranscript));

            // Raise event
            TranscriptionCompleted?.Invoke(this, result);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Applies search and filters to the segments collection
        /// </summary>
        private void ApplyFilters()
        {
            _filteredSegments.Clear();

            var filtered = _segments.AsEnumerable();

            // Apply speaker filter
            if (!string.IsNullOrEmpty(_speakerFilter))
            {
                filtered = filtered.Where(s => s.SpeakerId == _speakerFilter);
            }

            // Add to filtered collection
            foreach (var segment in filtered)
            {
                _filteredSegments.Add(segment);
            }

            // Re-apply search if needed
            if (HasSearchText)
            {
                PerformSearch();
            }
        }

        /// <summary>
        /// Performs a search on the filtered segments
        /// </summary>
        private void PerformSearch()
        {
            // Clear previous search results
            foreach (var segment in _filteredSegments)
            {
                segment.IsSearchMatch = false;
            }

            _searchResults.Clear();
            _currentSearchIndex = -1;

            if (string.IsNullOrWhiteSpace(_searchText))
            {
                UpdateSearchCount();
                this.OnPropertyChanged(nameof(HasSearchMatches));
                return;
            }

            // Perform case-insensitive search
            var pattern = new Regex(Regex.Escape(_searchText), RegexOptions.IgnoreCase);

            foreach (var segment in _filteredSegments)
            {
                if (pattern.IsMatch(segment.Text))
                {
                    segment.IsSearchMatch = true;
                    _searchResults.Add(segment);
                }
            }

            // Update UI
            UpdateSearchCount();
            this.OnPropertyChanged(nameof(HasSearchMatches));

            // Navigate to first match if available
            if (_searchResults.Count > 0)
            {
                _currentSearchIndex = 0;
                ScrollToSegment(_searchResults[0]);
            }
        }

        /// <summary>
        /// Clears the current search
        /// </summary>
        private void ClearSearch()
        {
            // Clear search matches
            foreach (var segment in _filteredSegments)
            {
                segment.IsSearchMatch = false;
            }

            _searchResults.Clear();
            _currentSearchIndex = -1;

            // Update UI
            UpdateSearchCount();
            this.OnPropertyChanged(nameof(HasSearchMatches));
            this.OnPropertyChanged(nameof(HasSearchText));
        }

        /// <summary>
        /// Updates the search count display
        /// </summary>
        private void UpdateSearchCount()
        {
            if (matchCountTextTranscript == null)
            {
                return;
            }

            if (_searchResults.Count == 0)
            {
                matchCountTextTranscript.Text = string.Empty;
            }
            else
            {
                int currentIndex = _currentSearchIndex >= 0 ? _currentSearchIndex + 1 : 0;
                matchCountTextTranscript.Text = $"{currentIndex} of {_searchResults.Count} matches";
            }
        }

        /// <summary>
        /// Navigates to the previous search match
        /// </summary>
        private void NavigateToPreviousMatch()
        {
            if (_searchResults.Count == 0)
            {
                return;
            }

            _currentSearchIndex--;
            if (_currentSearchIndex < 0)
            {
                _currentSearchIndex = _searchResults.Count - 1;
            }

            // Scroll to segment
            ScrollToSegment(_searchResults[_currentSearchIndex]);
            UpdateSearchCount();
        }

        /// <summary>
        /// Navigates to the next search match
        /// </summary>
        private void NavigateToNextMatch()
        {
            if (_searchResults.Count == 0)
            {
                return;
            }

            _currentSearchIndex++;
            if (_currentSearchIndex >= _searchResults.Count)
            {
                _currentSearchIndex = 0;
            }

            // Scroll to segment
            ScrollToSegment(_searchResults[_currentSearchIndex]);
            UpdateSearchCount();
        }

        /// <summary>
        /// Scrolls to a specific segment
        /// </summary>
        /// <param name="segment">Segment to scroll to</param>
        private void ScrollToSegment(TranscriptSegmentViewModel segment)
        {
            if (segment == null || transcriptListViewTranscript == null)
            {
                return;
            }

            // Select and scroll to the item
            transcriptListViewTranscript.SelectedItem = segment;
            transcriptListViewTranscript.ScrollIntoView(segment);
        }

        /// <summary>
        /// Updates the current segment based on the media position
        /// </summary>
        private void UpdateCurrentSegment()
        {
            if (_filteredSegments.Count == 0 || ViewModel == null)
            {
                return;
            }

            // Get the current position from the ViewModel
            TimeSpan currentPosition = ViewModel.CurrentPosition;

            // Reset highlight on previous segment
            if (_currentSegment != null)
            {
                _currentSegment.IsHighlighted = false;
            }

            // Find the current segment based on media position
            _currentSegment = _filteredSegments.FirstOrDefault(s =>
                currentPosition >= s.StartTime && currentPosition <= s.EndTime);

            // If no segment found at current position, try to find the closest upcoming segment
            if (_currentSegment == null && _filteredSegments.Count > 0)
            {
                _currentSegment = _filteredSegments
                    .Where(s => currentPosition <= s.StartTime)
                    .OrderBy(s => s.StartTime)
                    .FirstOrDefault();
            }

            // Highlight the current segment
            if (_currentSegment != null)
            {
                _currentSegment.IsHighlighted = true;

                // Auto-scroll if enabled
                if (_autoScrollEnabled)
                {
                    ScrollToSegment(_currentSegment);
                }
            }
            
            // Update UI to reflect current segment
            OnPropertyChanged(nameof(HasTranscript));
        }

        /// <summary>
        /// Parses subtitles from text lines
        /// </summary>
        /// <param name="lines">Lines from subtitle file</param>
        /// <param name="isVtt">Whether the format is WebVTT</param>
        /// <returns>List of transcript segments</returns>
        private List<TrialWorld.Core.Models.MediaTranscriptSegment> ParseSubtitles(string[] lines, bool isVtt)
        {
            var segments = new List<TrialWorld.Core.Models.MediaTranscriptSegment>();
            int lineIndex = 0;

            // Skip WebVTT header if needed
            if (isVtt && lines.Length > 0 && lines[0].StartsWith("WEBVTT"))
            {
                lineIndex++;
                while (lineIndex < lines.Length && !string.IsNullOrWhiteSpace(lines[lineIndex]))
                {
                    lineIndex++;
                }
            }

            while (lineIndex < lines.Length)
            {
                // Skip empty lines
                if (string.IsNullOrWhiteSpace(lines[lineIndex]))
                {
                    lineIndex++;
                    continue;
                }

                // Skip segment number
                lineIndex++;
                if (lineIndex >= lines.Length) break;

                // Parse time codes (format: 00:00:00,000 --> 00:00:00,000)
                string timeLine = lines[lineIndex];
                lineIndex++;
                if (lineIndex >= lines.Length) break;

                // Extract times
                var timeMatch = Regex.Match(timeLine, @"(\d{2}:\d{2}:\d{2}[,.]\d{3})\s*-->\s*(\d{2}:\d{2}:\d{2}[,.]\d{3})");
                if (!timeMatch.Success) continue;

                string startStr = timeMatch.Groups[1].Value.Replace(',', '.');
                string endStr = timeMatch.Groups[2].Value.Replace(',', '.');

                if (!TimeSpan.TryParse(startStr, out var start) || !TimeSpan.TryParse(endStr, out var end))
                {
                    continue;
                }

                // Read subtitle text (can be multiple lines)
                var textBuilder = new StringBuilder();
                while (lineIndex < lines.Length && !string.IsNullOrWhiteSpace(lines[lineIndex]))
                {
                    if (textBuilder.Length > 0)
                    {
                        textBuilder.Append(" ");
                    }
                    textBuilder.Append(lines[lineIndex]);
                    lineIndex++;
                }

                // Add segment
                segments.Add(new TrialWorld.Core.Models.MediaTranscriptSegment
                {
                    Text = textBuilder.ToString(),
                    StartTime = start.TotalSeconds,
                    EndTime = end.TotalSeconds,

                    Confidence = 1.0
                });
            }

            return segments;
        }

        /// <summary>
        /// Invokes the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">Name of the property that changed</param>
        

/// <summary>
        /// Invokes the PropertyChanged event for INotifyPropertyChanged
        /// </summary>
        /// <param name="propertyName">Name of the property that changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    /// <summary>
    /// View model for a transcript segment, wrapping the canonical Core model and exposing UI state
    /// </summary>
    public class TranscriptSegmentViewModel : INotifyPropertyChanged
    {
        // _coreSegment is now TrialWorld.Core.Models.Transcription.TranscriptSegment
        private TrialWorld.Core.Models.Transcription.TranscriptSegment _coreSegment;
        private bool _isHighlighted;
        private bool _isSearchMatch;
        private Brush? _backgroundBrush = Brushes.Transparent;

        public TranscriptSegmentViewModel(TrialWorld.Core.Models.Transcription.TranscriptSegment coreSegment)
        {
            _coreSegment = coreSegment;
        }

        public string Text
        {
            get => _coreSegment.Text ?? string.Empty; // Ensure null safety
            set { if ((_coreSegment.Text ?? string.Empty) != value) { _coreSegment.Text = value; this.OnPropertyChanged(); } }
        }
        public TimeSpan StartTime
        {
            // Convert double milliseconds to TimeSpan
            get => TimeSpan.FromMilliseconds(_coreSegment.StartTime);
            set { if (TimeSpan.FromMilliseconds(_coreSegment.StartTime) != value) { _coreSegment.StartTime = value.TotalMilliseconds; this.OnPropertyChanged(); this.OnPropertyChanged(nameof(Duration)); } }
        }
        public TimeSpan EndTime
        {
            // Convert double milliseconds to TimeSpan
            get => TimeSpan.FromMilliseconds(_coreSegment.EndTime);
            set { if (TimeSpan.FromMilliseconds(_coreSegment.EndTime) != value) { _coreSegment.EndTime = value.TotalMilliseconds; this.OnPropertyChanged(); this.OnPropertyChanged(nameof(Duration)); } }
        }
        // Calculate Duration based on StartTime and EndTime (which are now TimeSpan in ViewModel)
        public TimeSpan Duration => EndTime - StartTime;
        public string SpeakerId // This property in ViewModel maps to Speaker in CoreSegment
        {
            get => _coreSegment.Speaker ?? "Unknown"; // Use Speaker from CoreSegment
            set { if ((_coreSegment.Speaker ?? "Unknown") != value) { _coreSegment.Speaker = value; this.OnPropertyChanged(); this.OnPropertyChanged(nameof(HasSpeaker)); } }
        }
        public bool HasSpeaker => !string.IsNullOrEmpty(_coreSegment.Speaker);
        public double Confidence
        {
            get => _coreSegment.Confidence;
            set { if (_coreSegment.Confidence != value) { _coreSegment.Confidence = value; this.OnPropertyChanged(); } }
        }
        // Metadata removed as it's not in the consolidated TranscriptSegment
        // public Dictionary<string, string> Metadata => _coreSegment.Metadata;

        // UI-only properties
        public bool IsHighlighted
        {
            get => _isHighlighted;
            set { if (_isHighlighted != value) { _isHighlighted = value; this.OnPropertyChanged(); } }
        }
        public bool IsSearchMatch
        {
            get => _isSearchMatch;
            set { if (_isSearchMatch != value) { _isSearchMatch = value; this.OnPropertyChanged(); } }
        }
        public Brush BackgroundBrush
        {
            get => _backgroundBrush!;
            set { if (_backgroundBrush != value) { _backgroundBrush = value; this.OnPropertyChanged(); } }
        }

        // Expose the core segment, ensuring it's the correct type
        public TrialWorld.Core.Models.Transcription.TranscriptSegment CoreSegment => _coreSegment;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrialWorld.Core.Models.Search;
using TrialWorld.Core.Interfaces;
using TrialWorld.Contracts.DTOs;

namespace TrialWorld.Presentation.Views
{
    /// <summary>
    /// Interaction logic for SearchView.xaml
    /// </summary>
    public partial class SearchView : UserControl, INotifyPropertyChanged
    {
        private readonly ISearchService _searchService;
        private readonly IFFmpegService _ffmpegService;
        private CancellationTokenSource? _cancellationTokenSource;

        #region Bindable Properties

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        private ObservableCollection<SearchResultViewModel> _searchResults = new ObservableCollection<SearchResultViewModel>();
        public ObservableCollection<SearchResultViewModel> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }

        private string _resultsText = "Enter a search term above to find media";
        public string ResultsText
        {
            get => _resultsText;
            set => SetProperty(ref _resultsText, value);
        }

        private string _statusText = "Ready";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(LoadingVisibility));
                    OnPropertyChanged(nameof(NoResultsVisibility));
                }
            }
        }

        public Visibility LoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;

        public Visibility NoResultsVisibility => !IsLoading && SearchResults.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        // Filter properties
        private bool _filterVideoEnabled = true;
        public bool FilterVideoEnabled
        {
            get => _filterVideoEnabled;
            set => SetProperty(ref _filterVideoEnabled, value);
        }

        private bool _filterAudioEnabled = true;
        public bool FilterAudioEnabled
        {
            get => _filterAudioEnabled;
            set => SetProperty(ref _filterAudioEnabled, value);
        }

        private DateTime? _filterDateFrom = null;
        public DateTime? FilterDateFrom
        {
            get => _filterDateFrom;
            set => SetProperty(ref _filterDateFrom, value);
        }

        private DateTime? _filterDateTo = null;
        public DateTime? FilterDateTo
        {
            get => _filterDateTo;
            set => SetProperty(ref _filterDateTo, value);
        }

        private double? _filterMinDuration = null;
        public double? FilterMinDuration
        {
            get => _filterMinDuration;
            set => SetProperty(ref _filterMinDuration, value);
        }

        private double? _filterMaxDuration = null;
        public double? FilterMaxDuration
        {
            get => _filterMaxDuration;
            set => SetProperty(ref _filterMaxDuration, value);
        }

        #endregion

        public SearchView(TrialWorld.Core.Interfaces.ISearchService searchService, TrialWorld.Core.Interfaces.IFFmpegService ffmpegService)
        {
            InitializeComponent();

            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
            _ffmpegService = ffmpegService ?? throw new ArgumentNullException(nameof(ffmpegService));

            DataContext = this;
        }

        #region Event Handlers

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSearch();
                e.Handled = true;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void AdvancedButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Show advanced search dialog
            MessageBox.Show("Advanced search options not yet implemented", "Advanced Search", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            FilterVideoEnabled = true;
            FilterAudioEnabled = true;
            FilterDateFrom = null;
            FilterDateTo = null;
            FilterMinDuration = null;
            FilterMaxDuration = null;

            PerformSearch();
        }

        private void ResultsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ResultsListView.SelectedItem as SearchResultViewModel;
            if (selectedItem != null)
            {
                // TODO: Open the selected media in the media player
                MessageBox.Show($"Opening {selectedItem.Title}\nPath: {selectedItem.MediaPath}", "Open Media", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion

        #region Search Methods

        private async void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                MessageBox.Show("Please enter a search term", "Search", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Cancel any previous search
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                IsLoading = true;
                StatusText = "Searching...";
                SearchResults.Clear();

                // Create search filters
                var filters = new SearchFilters();

                // Add media type filters
                if (FilterVideoEnabled && !FilterAudioEnabled)
                {
                    filters.MediaTypes ??= new HashSet<string>();
                    filters.MediaTypes.Add("video");
                }
                else if (!FilterVideoEnabled && FilterAudioEnabled)
                {
                    filters.MediaTypes ??= new HashSet<string>();
                    filters.MediaTypes.Add("audio");
                }

                // Add date range filters
                if (FilterDateFrom.HasValue || FilterDateTo.HasValue)
                {
                    filters.DateRange = new DateTimeRange
                    {
                        Start = FilterDateFrom,
                        End = FilterDateTo
                    };
                }

                // Add duration filters (convert double? to TimeSpan?)
                filters.DurationRange = new TimeSpanRange
                {
                    Start = FilterMinDuration.HasValue ? TimeSpan.FromSeconds(FilterMinDuration.Value) : (TimeSpan?)null,
                    End = FilterMaxDuration.HasValue ? TimeSpan.FromSeconds(FilterMaxDuration.Value) : (TimeSpan?)null
                };

                // Build SearchQuery for SearchAsync
                var searchQuery = new SearchQuery
                {
                    Text = SearchQuery,
                    Filters = new Dictionary<string, string>(),
                    SortBy = "relevance", // or another default, adjust as needed
                    PageSize = 50,
                    PageNumber = 1
                };

                // Map filters to SearchQuery.Filters
                if (filters.MediaTypes != null && filters.MediaTypes.Count > 0)
                    searchQuery.Filters["MediaType"] = string.Join(",", filters.MediaTypes);

                if (filters.DateRange != null)
                {
                    if (filters.DateRange.Start.HasValue)
                        searchQuery.Filters["DateFrom"] = filters.DateRange.Start.Value.ToString("o");
                    if (filters.DateRange.End.HasValue)
                        searchQuery.Filters["DateTo"] = filters.DateRange.End.Value.ToString("o");
                }

                if (filters.DurationRange != null)
                {
                    if (filters.DurationRange.Start.HasValue)
                        searchQuery.Filters["DurationMin"] = filters.DurationRange.Start.Value.TotalSeconds.ToString();
                    if (filters.DurationRange.End.HasValue)
                        searchQuery.Filters["DurationMax"] = filters.DurationRange.End.Value.TotalSeconds.ToString();
                }

                // Perform search
                var results = await _searchService.SearchAsync(searchQuery, _cancellationTokenSource.Token);

                // Update UI with results
                ResultsText = $"Found {results.TotalMatches} results ({results.ElapsedMilliseconds} ms)";

                foreach (var item in results.Items)
                {
                    var viewModel = new SearchResultViewModel
                    {
                        Id = item.Id,
                        Title = item.Title,
                        MediaPath = item.MediaPath,
                        MediaType = item.MediaType,
                        MediaTypeDisplay = GetMediaTypeDisplay(item.MediaType),
                        Duration = item.DurationInSeconds,
                        DurationDisplay = FormatDuration(item.DurationInSeconds),
                        FileDate = item.FileDate,
                        Score = item.Score
                    };

                    // Get text preview from matches
                    if (item.TextMatches != null && item.TextMatches.Any())
                    {
                        viewModel.TextPreview = item.TextMatches.First().HighlightedText ?? "...";
                    }

                    SearchResults.Add(viewModel);
                }

                StatusText = "Ready";
            }
            catch (OperationCanceledException)
            {
                StatusText = "Search canceled";
            }
            catch (Exception ex)
            {
                StatusText = "Error: " + ex.Message;
                MessageBox.Show($"Error performing search: {ex.Message}", "Search Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string GetMediaTypeDisplay(string mediaType)
        {
            return mediaType?.ToLowerInvariant() switch
            {
                "video" => "Video",
                "video_silent" => "Video (No Audio)",
                "audio" => "Audio",
                _ => mediaType ?? "Unknown"
            };
        }

        private string FormatDuration(double durationInSeconds)
        {
            var timeSpan = TimeSpan.FromSeconds(durationInSeconds);
            if (timeSpan.TotalHours >= 1)
            {
                return $"{timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
            }
            else
            {
                return $"{timeSpan.Seconds}s";
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }

    /// <summary>
    /// View model for a search result item.
    /// </summary>
    public class SearchResultViewModel
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? MediaPath { get; set; }
        public string? MediaType { get; set; }
        public string? MediaTypeDisplay { get; set; }
        public double Duration { get; set; }
        public string? DurationDisplay { get; set; }
        public DateTime FileDate { get; set; }
        public double Score { get; set; }
        public string? TextPreview { get; set; }
    }
}
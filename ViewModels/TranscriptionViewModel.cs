using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TrialWorld.AssemblyAIDiagnostic.Models;
using TrialWorld.AssemblyAIDiagnostic.Repositories;
using TrialWorld.AssemblyAIDiagnostic.Services;
using TrialWorld.Infrastructure.Transcription.Configuration;

namespace TrialWorld.AssemblyAIDiagnostic.ViewModels
{
    /// <summary>
    /// ViewModel for managing transcription operations in the UI.
    /// </summary>
    public class TranscriptionViewModel : INotifyPropertyChanged
    {
        private readonly IAssemblyAIDirectApiService _transcriptionService;
        private readonly IAudioExtractionService _audioExtractionService;
        private readonly ITranscriptionRepository _transcriptionRepository;
        private readonly ILogger<TranscriptionViewModel> _logger;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isTranscribing;
        private string _statusMessage = string.Empty;
        private double _progressValue;
        private string _selectedFilePath = string.Empty;
        private string _transcriptionText = string.Empty;
        private bool _isSearching;
        private string _searchQuery = string.Empty;
        private ObservableCollection<MediaTranscript> _searchResults = new ObservableCollection<MediaTranscript>();

        /// <summary>
        /// Gets the collection of transcription jobs.
        /// </summary>
        public ObservableCollection<TranscriptionJobModel> TranscriptionQueue { get; } = new ObservableCollection<TranscriptionJobModel>();

        /// <summary>
        /// Gets the command for selecting a file to transcribe.
        /// </summary>
        public ICommand SelectFileCommand { get; }

        /// <summary>
        /// Gets the command for starting transcription.
        /// </summary>
        public ICommand StartTranscriptionCommand { get; }

        /// <summary>
        /// Gets the command for cancelling transcription.
        /// </summary>
        public ICommand CancelTranscriptionCommand { get; }

        /// <summary>
        /// Gets or sets a value indicating whether transcription is in progress.
        /// </summary>
        public bool IsTranscribing
        {
            get => _isTranscribing;
            set
            {
                if (_isTranscribing != value)
                {
                    _isTranscribing = value;
                    OnPropertyChanged(nameof(IsTranscribing));
                    OnPropertyChanged(nameof(IsNotTranscribing));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether transcription is not in progress.
        /// </summary>
        public bool IsNotTranscribing => !_isTranscribing;

        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged(nameof(StatusMessage));
                }
            }
        }

        /// <summary>
        /// Gets or sets the progress value.
        /// </summary>
        public double ProgressValue
        {
            get => _progressValue;
            set
            {
                if (_progressValue != value)
                {
                    _progressValue = value;
                    OnPropertyChanged(nameof(ProgressValue));
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected file path.
        /// </summary>
        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                if (_selectedFilePath != value)
                {
                    _selectedFilePath = value;
                    OnPropertyChanged(nameof(SelectedFilePath));
                    OnPropertyChanged(nameof(HasSelectedFile));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether a file has been selected.
        /// </summary>
        public bool HasSelectedFile => !string.IsNullOrEmpty(_selectedFilePath);

        /// <summary>
        /// Gets or sets the transcription text.
        /// </summary>
        public string TranscriptionText
        {
            get => _transcriptionText;
            set
            {
                if (_transcriptionText != value)
                {
                    _transcriptionText = value;
                    OnPropertyChanged(nameof(TranscriptionText));
                }
            }
        }

        /// <summary>
        /// Gets the collection of search results.
        /// </summary>
        public ObservableCollection<MediaTranscript> SearchResults
        {
            get => _searchResults;
            set
            {
                if (_searchResults != value)
                {
                    _searchResults = value;
                    OnPropertyChanged(nameof(SearchResults));
                }
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether a search is in progress.
        /// </summary>
        public bool IsSearching
        {
            get => _isSearching;
            set
            {
                if (_isSearching != value)
                {
                    _isSearching = value;
                    OnPropertyChanged(nameof(IsSearching));
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the search query.
        /// </summary>
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged(nameof(SearchQuery));
                }
            }
        }
        
        /// <summary>
        /// Gets the command for searching transcripts.
        /// </summary>
        public ICommand SearchCommand { get; }
        
        /// <summary>
        /// Gets the command for rebuilding the search index.
        /// </summary>
        public ICommand RebuildIndexCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranscriptionViewModel"/> class.
        /// </summary>
        /// <param name="transcriptionService">The transcription service.</param>
        /// <param name="audioExtractionService">The audio extraction service.</param>
        /// <param name="transcriptionRepository">The transcription repository.</param>
        /// <param name="logger">The logger.</param>
        public TranscriptionViewModel(
            IAssemblyAIDirectApiService transcriptionService,
            IAudioExtractionService audioExtractionService,
            ITranscriptionRepository transcriptionRepository,
            ILogger<TranscriptionViewModel> logger)
        {
            _transcriptionService = transcriptionService ?? throw new ArgumentNullException(nameof(transcriptionService));
            _audioExtractionService = audioExtractionService ?? throw new ArgumentNullException(nameof(audioExtractionService));
            _transcriptionRepository = transcriptionRepository ?? throw new ArgumentNullException(nameof(transcriptionRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            SelectFileCommand = new RelayCommand(_ => SelectFile());
            StartTranscriptionCommand = new RelayCommand(_ => StartTranscriptionAsync(), _ => HasSelectedFile && !IsTranscribing);
            CancelTranscriptionCommand = new RelayCommand(_ => CancelTranscription(), _ => IsTranscribing);
            SearchCommand = new RelayCommand(_ => SearchTranscriptsAsync(), _ => !string.IsNullOrWhiteSpace(SearchQuery) && !IsSearching);
            RebuildIndexCommand = new RelayCommand(_ => RebuildSearchIndexAsync(), _ => !IsSearching);
        }

        /// <summary>
        /// Selects a file to transcribe.
        /// </summary>
        private void SelectFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Audio Files|*.mp3;*.wav;*.m4a;*.flac;*.aac|All Files|*.*",
                Title = "Select Audio File"
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedFilePath = dialog.FileName;
                StatusMessage = $"Selected file: {System.IO.Path.GetFileName(SelectedFilePath)}";
                _logger.LogInformation("File selected for transcription: {FilePath}", SelectedFilePath);
            }
        }

        /// <summary>
        /// Starts the transcription process asynchronously.
        /// </summary>
        private async void StartTranscriptionAsync()
        {
            if (string.IsNullOrEmpty(SelectedFilePath) || !System.IO.File.Exists(SelectedFilePath))
            {
                StatusMessage = "Please select a valid file first.";
                return;
            }

            try
            {
                IsTranscribing = true;
                StatusMessage = "Starting transcription...";
                ProgressValue = 0;
                TranscriptionText = string.Empty;

                _cancellationTokenSource = new CancellationTokenSource();
                var token = _cancellationTokenSource.Token;

                // Create a new transcription job and add it to the queue
                var job = new TranscriptionJobModel
                {
                    FileName = Path.GetFileName(SelectedFilePath),
                    FilePath = SelectedFilePath,
                    Status = TranscriptionStatus.NotStarted,
                    SubmittedAt = DateTime.Now
                };

                Application.Current.Dispatcher.Invoke(() => TranscriptionQueue.Add(job));
                
                // Check if this is a video file that needs audio extraction
                string fileToUpload = SelectedFilePath;
                if (_audioExtractionService.IsVideoFile(SelectedFilePath))
                {
                    // Step 1: Extract audio from video
                    StatusMessage = "Extracting audio from video...";
                    ProgressValue = 5;
                    job.Status = TranscriptionStatus.Extracting;
                    UpdateJob(job);
                    
                    fileToUpload = await _audioExtractionService.ExtractAudioAsync(SelectedFilePath, "mp3", token).ConfigureAwait(true);
                    
                    if (token.IsCancellationRequested)
                    {
                        job.Status = TranscriptionStatus.Cancelled;
                        UpdateJob(job);
                        StatusMessage = "Transcription cancelled.";
                        return;
                    }
                }

                // Step 2: Upload the file
                StatusMessage = "Uploading file...";
                ProgressValue = 20;
                job.Status = TranscriptionStatus.Uploading;
                UpdateJob(job);

                var uploadUrl = await _transcriptionService.UploadFileAsync(fileToUpload, token).ConfigureAwait(true);
                
                if (token.IsCancellationRequested)
                {
                    job.Status = TranscriptionStatus.Cancelled;
                    UpdateJob(job);
                    StatusMessage = "Transcription cancelled.";
                    return;
                }

                // Step 3: Submit for transcription
                StatusMessage = "Submitting transcription request...";
                ProgressValue = 40;
                job.Status = TranscriptionStatus.Queued;
                UpdateJob(job);

                var transcriptionId = await _transcriptionService.SubmitTranscriptionAsync(uploadUrl, token).ConfigureAwait(true);
                job.TranscriptionId = transcriptionId;
                UpdateJob(job);

                if (token.IsCancellationRequested)
                {
                    job.Status = TranscriptionStatus.Cancelled;
                    UpdateJob(job);
                    StatusMessage = "Transcription cancelled.";
                    return;
                }

                // Step 4: Wait for completion
                StatusMessage = "Processing transcription...";
                ProgressValue = 60;
                job.Status = TranscriptionStatus.Processing;
                UpdateJob(job);

                var result = await _transcriptionService.WaitForCompletionAsync(transcriptionId, token).ConfigureAwait(true);

                if (token.IsCancellationRequested)
                {
                    job.Status = TranscriptionStatus.Cancelled;
                    UpdateJob(job);
                    StatusMessage = "Transcription cancelled.";
                    return;
                }

                // Step 5: Process the result
                if (result.Status == "completed")
                {
                    ProgressValue = 80;
                    StatusMessage = "Saving transcript to database...";
                    
                    // Create a MediaTranscript from the result
                    var transcript = new MediaTranscript
                    {
                        Id = transcriptionId,
                        MediaFilePath = SelectedFilePath,
                        FileName = Path.GetFileName(SelectedFilePath),
                        Text = result.Text ?? string.Empty,
                        CreatedDate = DateTime.Now,
                        DurationSeconds = result.AudioDuration ?? 0
                    };
                    
                    // Add segments if available
                    if (result.Utterances != null)
                    {
                        foreach (var utterance in result.Utterances)
                        {
                            transcript.Segments.Add(new TranscriptSegment
                            {
                                Text = utterance.Text ?? string.Empty,
                                Start = utterance.Start ?? 0,
                                End = utterance.End ?? 0
                            });
                        }
                    }
                    
                    // Save to repository
                    await _transcriptionRepository.SaveTranscriptAsync(SelectedFilePath, transcript, token).ConfigureAwait(true);
                    
                    ProgressValue = 100;
                    job.Status = TranscriptionStatus.Completed;
                    job.CompletedAt = DateTime.Now;
                    UpdateJob(job);

                    TranscriptionText = result.Text ?? string.Empty;
                    StatusMessage = "Transcription completed and saved to database.";
                    _logger.LogInformation("Transcription completed successfully for file: {FilePath}", SelectedFilePath);
                }
                else
                {
                    job.Status = TranscriptionStatus.Error;
                    job.ErrorMessage = result.Error ?? "Unknown error";
                    UpdateJob(job);

                    StatusMessage = $"Transcription failed: {result.Error}";
                    _logger.LogError("Transcription failed for file: {FilePath}. Error: {Error}", SelectedFilePath, result.Error);
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Transcription cancelled.";
                _logger.LogInformation("Transcription cancelled for file: {FilePath}", SelectedFilePath);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _logger.LogError(ex, "Error during transcription for file: {FilePath}", SelectedFilePath);

                var job = TranscriptionQueue.Count > 0 ? TranscriptionQueue[TranscriptionQueue.Count - 1] : null;
                if (job != null)
                {
                    job.Status = TranscriptionStatus.Error;
                    job.ErrorMessage = ex.Message;
                    UpdateJob(job);
                }
            }
            finally
            {
                IsTranscribing = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Cancels the current transcription.
        /// </summary>
        private void CancelTranscription()
        {
            _cancellationTokenSource?.Cancel();
            StatusMessage = "Cancelling transcription...";
            _logger.LogInformation("Transcription cancellation requested for file: {FilePath}", SelectedFilePath);
        }

        /// <summary>
        /// Updates a job in the transcription queue.
        /// </summary>
        /// <param name="job">The job to update.</param>
        private void UpdateJob(TranscriptionJobModel job)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var index = TranscriptionQueue.IndexOf(job);
                if (index >= 0)
                {
                    TranscriptionQueue[index] = job;
                }
            });
        }
        
        /// <summary>
        /// Searches transcripts asynchronously.
        /// </summary>
        private async void SearchTranscriptsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                return;
            }
            
            try
            {
                IsSearching = true;
                StatusMessage = $"Searching transcripts for '{SearchQuery}'...";
                
                var results = await _transcriptionRepository.SearchTranscriptsAsync(SearchQuery).ConfigureAwait(true);
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SearchResults.Clear();
                    foreach (var result in results)
                    {
                        SearchResults.Add(result);
                    }
                });
                
                StatusMessage = $"Found {SearchResults.Count} results for '{SearchQuery}'.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error searching transcripts: {ex.Message}";
                _logger.LogError(ex, "Error searching transcripts for query: {Query}", SearchQuery);
            }
            finally
            {
                IsSearching = false;
            }
        }
        
        /// <summary>
        /// Rebuilds the search index asynchronously.
        /// </summary>
        private async void RebuildSearchIndexAsync()
        {
            try
            {
                IsSearching = true;
                StatusMessage = "Rebuilding search index...";
                
                await _transcriptionRepository.RebuildSearchIndexAsync().ConfigureAwait(true);
                
                StatusMessage = "Search index rebuilt successfully.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error rebuilding search index: {ex.Message}";
                _logger.LogError(ex, "Error rebuilding search index");
            }
            finally
            {
                IsSearching = false;
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    /// <summary>
    /// Simple implementation of ICommand for relay commands.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execute action.</param>
        /// <param name="canExecute">The can execute predicate.</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Determines whether this command can execute.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>True if the command can execute; otherwise, false.</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}

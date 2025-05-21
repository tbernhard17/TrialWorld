using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading; // For DispatcherTimer
using System.Linq;
using System.Collections.ObjectModel;
// using System.Collections.Concurrent; // Not used in this version, can be added if needed
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Windows; // For Application.Current.Dispatcher
using System.Windows.Input; // Added for ICommand
// using Microsoft.Win32; // Typically used in IFileDialogService implementation, not directly in ViewModel
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions; // For NullLogger
// Assuming these are the correct namespaces for your core models and interfaces
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models; // Assuming MediaFile, TranscriptFileInfo, MediaMetadata, KeywordData, FaceData are here
using TrialWorld.Core.Common.Interfaces; // For IAppSettingsService
using TrialWorld.Core.Common.Extensions; // For AppSettingsExtensions
using TrialWorld.Core.Models.Transcription.Interfaces; // For ITranscriptionResult
using TrialWorld.Core.Services; // Assuming ITranscriptionDatabaseService is here
// Assuming these are for Presentation layer specific items
using TrialWorld.Presentation.Interfaces;
using TrialWorld.Presentation.Models; // For TrialWorld.Presentation.Models.TranscriptionQueueItem, TrialWorld.Presentation.Models.SearchResultItem
using TrialWorld.Presentation.ViewModels.Search; // Assuming SearchService is here or similar
// Remove Prism dependency
// using PrismIAsyncCommand = Prism.Commands.IAsyncCommand;
using TrialWorld.Presentation.Commands; // Assuming your ICommand implementations (e.g., RelayCommand) are here

using TrialWorld.Core.Models.Transcription;

namespace TrialWorld.Presentation.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Fields and Properties

        // Services
        private readonly IFileDialogService _fileDialogService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;
        private readonly IThemeService _themeService;
        private readonly IWindowManager _windowManager;
        private readonly IVersionService _versionService;
        private readonly IResourceMonitorService _resourceMonitorService;
        private readonly ITranscriptionService _transcriptionService;
        private readonly IDatabaseLoaderService _databaseLoader;
        private readonly ITranscriptionVerificationService _transcriptionVerification;
        private readonly IAppSettingsService _appSettingsService;
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly ISilenceDetectionService _silenceDetectionService; // Added for CourtroomMediaPlayerViewModel
        private readonly ILogger<CourtroomMediaPlayerViewModel> _courtroomMediaPlayerLogger; // Added for CourtroomMediaPlayerViewModel
        private readonly ILogger<MediaTranscriptViewModel> _mediaTranscriptViewModelLogger; // Added for MediaTranscriptViewModel

        public ITranscriptionService TranscriptionService => _transcriptionService;
        public CourtroomMediaPlayerViewModel? CourtroomMediaPlayerViewModel { get; private set; } // Renamed property
        public MediaTranscriptViewModel? MediaTranscriptViewModel { get; private set; }
        private readonly Dictionary<TrialWorld.Presentation.Models.TranscriptionQueueItem, CancellationTokenSource> _activeTranscriptions = new();
        private readonly object _activeTranscriptionsLock = new object();

        // Using the SearchService from the ViewModels namespace (not Search subnamespace)
        private readonly TrialWorld.Presentation.ViewModels.SearchService _searchService;
        private readonly IAppSettingsService _appSettings;

        private string _statusText = "Ready";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }
        private string _cpuUsage = "CPU: 0%";
        public string CpuUsage
        {
            get => _cpuUsage;
            set => SetProperty(ref _cpuUsage, value);
        }
        public ObservableCollection<TrialWorld.Presentation.Models.TranscriptionQueueItem> TranscriptionQueue { get; } = new();

        private ObservableCollection<TrialWorld.Presentation.Models.SearchResultItem> _searchResults = new();
        public ObservableCollection<TrialWorld.Presentation.Models.SearchResultItem> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }
        private string _currentTranscriptIdForSearch = string.Empty;
        private ITranscriptionResult? _currentTranscriptionForSearch;

        private bool _isProcessingAll;
        public bool CanProcessAll
        {
            get => !_isProcessingAll && TranscriptionQueue.Any(item => item.CanBeProcessed());
        }
        public Commands.IAsyncCommand ProcessAllCommand { get; }
        public Commands.IAsyncCommand CancelAllCommand { get; }
        public ICommand ClearQueueCommand { get; }
        public ICommand AddFileCommand { get; }
        public ICommand AddFolderCommand { get; }
        public ICommand RemoveSingleCommand { get; }
        public ICommand StopSingleCommand { get; }
        #endregion

        #region Constructors
        public MainWindowViewModel(
            IFileDialogService fileDialogService,
            INavigationService navigationService,
            INotificationService notificationService,
            IThemeService themeService,
            IWindowManager windowManager,
            IVersionService versionService,
            IResourceMonitorService resourceMonitorService,
            ITranscriptionService transcriptionService,
            IDatabaseLoaderService databaseLoader,
            ITranscriptionVerificationService transcriptionVerification,
            IAppSettingsService appSettingsService,
            TrialWorld.Presentation.ViewModels.SearchService searchService,
            ISilenceDetectionService silenceDetectionService, // Added parameter
            ILogger<CourtroomMediaPlayerViewModel> courtroomMediaPlayerLogger, // Added parameter
            ILogger<MediaTranscriptViewModel> mediaTranscriptViewModelLogger, // Added parameter
            ILogger<MainWindowViewModel> logger)
        {
            _fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
            _windowManager = windowManager ?? throw new ArgumentNullException(nameof(windowManager));
            _versionService = versionService ?? throw new ArgumentNullException(nameof(versionService));
            _resourceMonitorService = resourceMonitorService ?? throw new ArgumentNullException(nameof(resourceMonitorService));
            _transcriptionService = transcriptionService ?? throw new ArgumentNullException(nameof(transcriptionService));
            _databaseLoader = databaseLoader ?? throw new ArgumentNullException(nameof(databaseLoader));
            _transcriptionVerification = transcriptionVerification ?? throw new ArgumentNullException(nameof(transcriptionVerification));
            _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
            _appSettings = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
            _logger = logger ?? NullLogger<MainWindowViewModel>.Instance;
            _silenceDetectionService = silenceDetectionService ?? throw new ArgumentNullException(nameof(silenceDetectionService)); // Store injected service
            _courtroomMediaPlayerLogger = courtroomMediaPlayerLogger ?? NullLogger<CourtroomMediaPlayerViewModel>.Instance; // Store injected logger
            _mediaTranscriptViewModelLogger = mediaTranscriptViewModelLogger ?? NullLogger<MediaTranscriptViewModel>.Instance; // Store injected logger

            // Instantiate CourtroomMediaPlayerViewModel
            CourtroomMediaPlayerViewModel = new CourtroomMediaPlayerViewModel(_silenceDetectionService, _courtroomMediaPlayerLogger);
            
            // Instantiate MediaTranscriptViewModel
            MediaTranscriptViewModel = new MediaTranscriptViewModel(_transcriptionService, CourtroomMediaPlayerViewModel, _mediaTranscriptViewModelLogger);

            ProcessAllCommand = new AsyncRelayCommand(ProcessAllAsyncInternal, () => CanProcessAll);
            CancelAllCommand = new AsyncRelayCommand(CancelAllAsyncInternal, () =>
            {
                lock (_activeTranscriptionsLock)
                {
                    return _activeTranscriptions.Count > 0 || TranscriptionQueue.Any(i => i.IsQueuedOrProcessing());
                }
            });
            ClearQueueCommand = new RelayCommand(ClearQueueInternal, () => TranscriptionQueue.Count > 0 && !_isProcessingAll);
            AddFileCommand = new RelayCommand(ExecuteAddFile);
            AddFolderCommand = new RelayCommand(ExecuteAddFolder);
            RemoveSingleCommand = new RelayCommand<TrialWorld.Presentation.Models.TranscriptionQueueItem>(RemoveSingle);
            StopSingleCommand = new RelayCommand<TrialWorld.Presentation.Models.TranscriptionQueueItem>(StopSingle);

            TranscriptionQueue.CollectionChanged += (s, e) => UpdateCommandStates();
        }
        // Design-time constructor with simplified dependencies for development/preview
        public MainWindowViewModel() : this(
            new Services.FileDialogService(),
            new Services.NavigationService(),
            new Services.NotificationService(),
            new Services.ThemeService(),
            new Services.WindowManager(),
            new Services.VersionService(),
            new Services.ResourceMonitorService(),
            // Use a simple mock transcription service for design-time
            new Services.DesignTimeTranscriptionService(),
            new TrialWorld.Infrastructure.Services.DatabaseLoaderService(
                NullLogger<TrialWorld.Infrastructure.Services.DatabaseLoaderService>.Instance),
            new TrialWorld.Infrastructure.Services.TranscriptionVerificationService(
                NullLogger<TrialWorld.Infrastructure.Services.TranscriptionVerificationService>.Instance,
                new TrialWorld.Infrastructure.Services.DatabaseLoaderService(
                    NullLogger<TrialWorld.Infrastructure.Services.DatabaseLoaderService>.Instance),
                Microsoft.Extensions.Options.Options.Create(new TrialWorld.Core.Models.Configuration.TranscriptionPathSettings())),
            new TrialWorld.Core.Common.Services.AppSettingsService(),
            new TrialWorld.Presentation.ViewModels.SearchService(),
            new TrialWorld.Presentation.Services.DesignTimeSilenceDetectionService(), // Added design-time service
            NullLogger<CourtroomMediaPlayerViewModel>.Instance, // Added null logger for CourtroomMediaPlayerViewModel
            NullLogger<MediaTranscriptViewModel>.Instance, // Added null logger for MediaTranscriptViewModel
            NullLogger<MainWindowViewModel>.Instance)
        {
            TranscriptionQueue.Add(new TrialWorld.Presentation.Models.TranscriptionQueueItem { FilePath = "sample1.mp4", FileName = "sample1.mp4", Status = TranscriptionPhase.Queued.ToString(), CurrentPhase = TranscriptionPhase.Queued });
            TranscriptionQueue.Add(new TrialWorld.Presentation.Models.TranscriptionQueueItem { FilePath = "sample2.mp3", FileName = "sample2.mp3", Status = TranscriptionPhase.Processing.ToString(), CurrentPhase = TranscriptionPhase.Processing, SilenceDetectionProgress = 50 });

            _currentTranscriptIdForSearch = "design-time-transcript";
            _currentTranscriptionForSearch = new TranscriptionResult // This is a concrete class
            {
                Id = "design-time-id",
                Status = Core.Models.Transcription.TranscriptionStatus.Completed,
                Transcript = "This is a design-time mock transcript."
            };
            _isProcessingAll = false;
            OnPropertyChanged(nameof(CanProcessAll));
        }
        private void UpdateCommandStates()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                (ProcessAllCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (CancelAllCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (ClearQueueCommand as RelayCommand)?.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(CanProcessAll));
            });
        }
        #endregion

        #region Transcription Queue Methods

        private void ExecuteAddFile()
        {
            var filePath = _fileDialogService.OpenFile("Media Files|*.mp3;*.wav;*.mp4;*.mkv;*.avi;*.mov;*.wmv;*.aac|All Files|*.*", "Select Media File to Transcribe");
            if (!string.IsNullOrEmpty(filePath))
            {
                AddFileToQueue(filePath);
            }
        }
        private async void ExecuteAddFolder()
        {
            var folderPath = await _fileDialogService.ShowFolderBrowserDialogAsync("Select Folder Containing Media Files", null);
            if (!string.IsNullOrEmpty(folderPath))
            {
                AddDirectoryToQueue(folderPath);
            }
        }
        public void AddFileToQueue(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                _logger.LogWarning("Attempted to add invalid file path: {FilePath}", filePath);
                _notificationService.ShowWarning("Invalid file path or file does not exist.", "Add File Error");
                return;
            }
            string fileName = Path.GetFileName(filePath);
            lock (_activeTranscriptionsLock)
            {
                if (TranscriptionQueue.Any(item => item.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)) ||
                    _activeTranscriptions.Keys.Any(item => item.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogInformation("File {FileName} is already in the queue or being processed. Skipping add.", fileName);
                    StatusText = $"File {fileName} is already queued or processing.";
                    _notificationService.ShowNotification($"File '{fileName}' is already in the queue or being processed.", "File Exists");
                    return;
                }
            }
            var item = new TrialWorld.Presentation.Models.TranscriptionQueueItem
            {
                FilePath = filePath,
                FileName = fileName,
                Status = TranscriptionPhase.Queued.ToString(),
                CurrentPhase = TranscriptionPhase.Queued
            };
            TranscriptionQueue.Add(item);
            StatusText = $"Added: {item.FileName}";
            _logger.LogInformation("Added file to queue: {FilePath}", filePath);
        }
        public void AddDirectoryToQueue(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            {
                _logger.LogWarning("Attempted to add invalid directory path: {DirectoryPath}", directoryPath);
                _notificationService.ShowWarning("Invalid directory path or directory does not exist.", "Add Folder Error");
                return;
            }
            _logger.LogInformation("Scanning directory for media files: {DirectoryPath}", directoryPath);
            var defaultExtensions = new List<string> { ".mp3", ".wav", ".mp4", ".m4a", ".mkv", ".mov", ".avi" };
            var searchSettings = _appSettings.GetSearchSettings();
            var extensions = searchSettings.MediaExtensions != null ? searchSettings.MediaExtensions.ToList() : defaultExtensions;
            var supportedExtensions = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
            int filesAdded = 0;
            try
            {
                foreach (var filePath in Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.AllDirectories))
                {
                    if (supportedExtensions.Contains(Path.GetExtension(filePath)))
                    {
                        AddFileToQueue(filePath);
                        filesAdded++;
                    }
                }
                StatusText = $"Added {filesAdded} files from {Path.GetFileName(directoryPath)}.";
                _notificationService.ShowSuccess($"Added {filesAdded} media files from the selected folder.", "Folder Added");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning directory {DirectoryPath}", directoryPath);
                _notificationService.ShowError($"Error scanning folder: {ex.Message}", "Folder Scan Error");
            }
        }
        private async Task ProcessAllAsyncInternal()
        {
            if (_isProcessingAll)
            {
                _logger.LogInformation("ProcessAllAsyncInternal called while already processing.");
                return;
            }
            _isProcessingAll = true;
            OnPropertyChanged(nameof(CanProcessAll));
            (ProcessAllCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();

            StatusText = "Processing all eligible queue items...";
            _logger.LogInformation("Starting ProcessAllAsyncInternal...");

            List<TrialWorld.Presentation.Models.TranscriptionQueueItem> itemsToProcess;
            lock (_activeTranscriptionsLock)
            {
                itemsToProcess = TranscriptionQueue.Where(i => i.CanBeProcessed()).ToList();
            }
            if (!itemsToProcess.Any())
            {
                StatusText = "No new items to process in the queue.";
                _logger.LogInformation("No new items to process in ProcessAllAsyncInternal.");
                _isProcessingAll = false;
                OnPropertyChanged(nameof(CanProcessAll));
                (ProcessAllCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                return;
            }
            var processingTasks = new List<Task>();
            foreach (var item in itemsToProcess)
            {
                processingTasks.Add(ProcessSingleItemAsync(item));
            }
            try
            {
                await Task.WhenAll(processingTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Task.WhenAll in ProcessAllAsyncInternal. Individual task errors should be handled within ProcessSingleItemAsync.");
            }
            finally
            {
                _isProcessingAll = false;
                OnPropertyChanged(nameof(CanProcessAll));
                (ProcessAllCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                _logger.LogInformation("ProcessAllAsyncInternal finished.");
                StatusText = "Finished processing queue run.";
            }
        }
        private async Task ProcessSingleItemAsync(TrialWorld.Presentation.Models.TranscriptionQueueItem item)
        {
            var cts = new CancellationTokenSource();
            bool addedToActive = false;
            lock (_activeTranscriptionsLock)
            {
                if (!_activeTranscriptions.Keys.Any(activeItem => activeItem.FilePath.Equals(item.FilePath, StringComparison.OrdinalIgnoreCase)))
                {
                    _activeTranscriptions.Add(item, cts);
                    addedToActive = true;
                }
            }
            if (!addedToActive)
            {
                _logger.LogWarning("Item {FileName} was already added to active transcriptions by a concurrent call. Skipping this task instance.", item.FileName);
                return;
            }
            _logger.LogInformation("Starting processing for item: {FileName} with FilePath: {FilePath}", item.FileName, item.FilePath);

            try
            {
                item.FileHash = await _transcriptionVerification.CalculateFileHashAsync(item.FilePath);
                item.OutputFilePath = _transcriptionVerification.GetExpectedOutputPath(item.FilePath);

                bool alreadyTranscribedAndVerified = await _transcriptionVerification.IsAlreadyTranscribedAsync(item.FilePath, item.FileHash);
                if (alreadyTranscribedAndVerified)
                {
                    UpdateItemStatusOnUiThread(item, TranscriptionPhase.Completed, -1, "Already Transcribed & Verified");
                    item.IsVerified = true;
                    _logger.LogInformation("Skipping {FileName} - already transcribed and verified.", item.FileName);
                    return;
                }
                UpdateItemStatusOnUiThread(item, TranscriptionPhase.Processing, -1, "Preparing...");
                item.LastAttemptTime = DateTime.Now;
                item.AttemptCount++;

                await _transcriptionVerification.RegisterTranscriptionAsync(item.FilePath, item.FileHash, string.Empty, item.OutputFilePath, item.Status);

                var progressHandler = new Progress<TranscriptionProgressUpdate>(update =>
                {
                    UpdateItemStatusOnUiThread(item, update.Phase, update.ProgressPercent, update.ErrorMessage);
                });

                UpdateItemStatusOnUiThread(item, TranscriptionPhase.SilenceDetection, 0, "Starting Silence Detection...");
                
                // Create transcription config with silence detection settings
                var config = new TranscriptionConfig
                {
                    EnableSilenceDetection = true,
                    SilenceThresholdDb = -30,  // Default threshold in dB
                    MinimumSilenceDurationMs = 10000  // 10 seconds minimum silence duration
                };
                
                // Transcribe with progress and proper configuration
                ITranscriptionResult transcriptionResult = await _transcriptionService.TranscribeWithProgressAsync(
                    item.FilePath, 
                    config,
                    progressHandler, 
                    cts.Token);

                if (cts.Token.IsCancellationRequested)
                {
                    UpdateItemStatusOnUiThread(item, TranscriptionPhase.Cancelled, 0, "Cancelled by user");
                    await _transcriptionVerification.UpdateTranscriptionStatusAsync(item.FileHash, item.TranscriptionId, item.Status, false);
                    return;
                }
                if (transcriptionResult.Status == TranscriptionStatus.Completed && !string.IsNullOrEmpty(transcriptionResult.Id))
                {
                    item.TranscriptionId = transcriptionResult.Id;
                    UpdateItemStatusOnUiThread(item, TranscriptionPhase.Downloading, 0, "Downloading transcript...");

                    var exportSettings = _appSettings.GetExportSettings();
                    string defaultDownloadDir = exportSettings.DefaultExportPath;
                    if (string.IsNullOrEmpty(defaultDownloadDir) || !Directory.Exists(defaultDownloadDir))
                    {
                        defaultDownloadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TrialWorld", "Transcripts");
                        _logger.LogWarning("Default export path '{ConfiguredPath}' is invalid or not set, using fallback: {FallbackPath}", exportSettings.DefaultExportPath, defaultDownloadDir);
                    }
                    Directory.CreateDirectory(defaultDownloadDir);

                    string transcriptFileName = $"{Path.GetFileNameWithoutExtension(item.FileName)}_{item.TranscriptionId}.srt";
                    string downloadedFilePath = Path.Combine(defaultDownloadDir, transcriptFileName);

                    var downloadProgress = new Progress<TranscriptionProgressUpdate>(update =>
                    {
                        UpdateItemStatusOnUiThread(item, update.Phase, update.ProgressPercent, update.Message);
                    });

                    bool downloadSuccess = await _transcriptionService.DownloadTranscriptionFileAsync(
                                                item.TranscriptionId, 
                                                downloadedFilePath, 
                                                downloadProgress, 
                                                cts.Token);

                    if (cts.Token.IsCancellationRequested)
                    {
                        UpdateItemStatusOnUiThread(item, TranscriptionPhase.Cancelled, 0, "Download Cancelled");
                        await _transcriptionVerification.UpdateTranscriptionStatusAsync(item.FileHash, item.TranscriptionId, item.Status, false);
                        return;
                    }
                    if (downloadSuccess)
                    {
                        item.OutputFilePath = downloadedFilePath;
                        bool verified = await _transcriptionVerification.VerifyTranscriptionCompletionAsync(item.OutputFilePath);
                        if (verified)
                        {
                            UpdateItemStatusOnUiThread(item, TranscriptionPhase.Completed, 100, "Completed & Verified");
                            item.IsVerified = true;
                            _logger.LogInformation("Transcription & Download for {FileName} completed and verified at {OutputPath}", item.FileName, item.OutputFilePath);
                            _notificationService.ShowSuccess($"'{item.FileName}' transcribed and saved to '{item.OutputFilePath}'.", "Transcription Complete");
                        }
                        else
                        {
                            UpdateItemStatusOnUiThread(item, TranscriptionPhase.Failed, 0, "Verification Failed after download");
                            item.IsVerified = false;
                            _logger.LogWarning("Verification failed for {FileName} after download.", item.FileName);
                        }
                    }
                    else
                    {
                        UpdateItemStatusOnUiThread(item, TranscriptionPhase.Failed, 0, "Download Failed");
                        item.IsVerified = false;
                        _logger.LogError("Download failed for transcription ID {TranscriptionId} of file {FileName}", item.TranscriptionId, item.FileName);
                    }
                }
                else
                {
                    UpdateItemStatusOnUiThread(item, TranscriptionPhase.Failed, 0, transcriptionResult.Error ?? "Unknown transcription error");
                    item.IsVerified = false;
                    _logger.LogError("Transcription of {FileName} failed: {Error}", item.FileName, transcriptionResult.Error);
                }
                await _transcriptionVerification.UpdateTranscriptionStatusAsync(item.FileHash, item.TranscriptionId, item.Status, item.IsVerified);
            }
            catch (OperationCanceledException)
            {
                UpdateItemStatusOnUiThread(item, TranscriptionPhase.Cancelled, 0, "Operation Cancelled");
                item.IsVerified = false;
                _logger.LogInformation("Processing for {FileName} was cancelled.", item.FileName);
                await _transcriptionVerification.UpdateTranscriptionStatusAsync(item.FileHash, item.TranscriptionId, item.Status, item.IsVerified);
            }
            catch (Exception ex)
            {
                UpdateItemStatusOnUiThread(item, TranscriptionPhase.Failed, 0, $"Unhandled Error: {ex.Message.Split('\n')[0]}");
                item.IsVerified = false;
                _logger.LogError(ex, "Unhandled error processing transcription for {FileName}", item.FileName);
                await _transcriptionVerification.UpdateTranscriptionStatusAsync(item.FileHash, item.TranscriptionId, item.Status, item.IsVerified);
            }
            finally
            {
                lock (_activeTranscriptionsLock)
                {
                    _activeTranscriptions.Remove(item);
                }
                _logger.LogInformation("Finished processing for item: {FileName}. Final Status: {Status}", item.FileName, item.Status);
                System.Windows.Application.Current.Dispatcher.Invoke(UpdateCommandStates);
            }
        }
        private void UpdateItemStatusOnUiThread(TrialWorld.Presentation.Models.TranscriptionQueueItem item, TranscriptionPhase phase, double progressPercent = -1, string? statusMessageOverride = null)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                item.CurrentPhase = phase;
                string phaseName = phase.ToString();

                if (!string.IsNullOrEmpty(statusMessageOverride))
                {
                    item.Status = statusMessageOverride;
                }
                else
                {
                    item.Status = phaseName;
                }
                switch (phase)
                {
                    case TranscriptionPhase.Queued:
                        if (progressPercent >= 0) item.OverallProgress = progressPercent;
                        break;
                    case TranscriptionPhase.SilenceDetection:
                        if (progressPercent >= 0) item.SilenceDetectionProgress = progressPercent;
                        if (string.IsNullOrEmpty(statusMessageOverride)) item.Status = $"Silence Detection ({item.SilenceDetectionProgress:F0}%)";
                        break;
                    case TranscriptionPhase.AudioExtraction:
                        if (progressPercent >= 0) item.AudioExtractionProgress = progressPercent;
                        if (string.IsNullOrEmpty(statusMessageOverride)) item.Status = $"Extracting Audio ({item.AudioExtractionProgress:F0}%)";
                        break;
                    case TranscriptionPhase.Uploading:
                        if (progressPercent >= 0) item.UploadProgress = progressPercent;
                        if (string.IsNullOrEmpty(statusMessageOverride)) item.Status = $"Uploading ({item.UploadProgress:F0}%)";
                        break;
                    case TranscriptionPhase.Submitted:
                        if (progressPercent >= 0) item.TranscribeProgress = progressPercent;
                        if (string.IsNullOrEmpty(statusMessageOverride)) item.Status = "Submitted to Provider";
                        break;
                    case TranscriptionPhase.Processing:
                        if (progressPercent >= 0) item.TranscribeProgress = progressPercent;
                        if (string.IsNullOrEmpty(statusMessageOverride)) item.Status = $"Transcribing ({item.TranscribeProgress:F0}%)";
                        break;
                    case TranscriptionPhase.Downloading:
                        if (progressPercent >= 0) item.DownloadProgress = progressPercent;
                        if (string.IsNullOrEmpty(statusMessageOverride)) item.Status = $"Downloading ({item.DownloadProgress:F0}%)";
                        break;
                    case TranscriptionPhase.Completed:
                        item.SilenceDetectionProgress = 100;
                        item.AudioExtractionProgress = 100;
                        item.UploadProgress = 100;
                        item.TranscribeProgress = 100;
                        item.DownloadProgress = 100;
                        item.OverallProgress = 100;
                        if (string.IsNullOrEmpty(statusMessageOverride)) item.Status = "Completed";
                        break;
                    case TranscriptionPhase.Failed:
                        // Status message override should contain the error
                        break;
                    case TranscriptionPhase.Cancelled:
                        // Status message override should indicate cancellation
                        break;
                }
                _logger.LogDebug("UI Update for {FileName}: Phase={CurrentPhase}, Progress={Progress}%, Status='{Status}'",
                                 item.FileName, item.CurrentPhase, progressPercent, item.Status);
            });
        }
        private async Task CancelAllAsyncInternal()
        {
            StatusText = "Cancelling all active and queued transcriptions...";
            _logger.LogInformation("Attempting to cancel all transcriptions.");

            List<KeyValuePair<TrialWorld.Presentation.Models.TranscriptionQueueItem, CancellationTokenSource>> activeToCancel;
            lock (_activeTranscriptionsLock)
            {
                activeToCancel = _activeTranscriptions.ToList();
            }
            _logger.LogInformation("Found {ActiveCount} active transcriptions to cancel.", activeToCancel.Count);
            foreach (var kvp in activeToCancel)
            {
                var item = kvp.Key;
                var cts = kvp.Value;
                try
                {
                    if (!cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                        _logger.LogInformation("Cancellation requested for active item: {FileName}", item.FileName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error requesting cancellation for active item {FileName}", item.FileName);
                }
            }
            List<TrialWorld.Presentation.Models.TranscriptionQueueItem> queuedItemsToCancel;
            lock (_activeTranscriptionsLock)
            {
                queuedItemsToCancel = TranscriptionQueue
                    .Where(i => !_activeTranscriptions.ContainsKey(i) && (i.CurrentPhase == TranscriptionPhase.Queued || i.Status == TranscriptionPhase.Queued.ToString()))
                    .ToList();
            }
            _logger.LogInformation("Found {QueuedCount} queued items to mark as cancelled.", queuedItemsToCancel.Count);
            foreach (var item in queuedItemsToCancel)
            {
                UpdateItemStatusOnUiThread(item, TranscriptionPhase.Cancelled, 0, "Cancelled from queue");
                if (!string.IsNullOrEmpty(item.FileHash))
                {
                     await _transcriptionVerification.UpdateTranscriptionStatusAsync(item.FileHash, item.TranscriptionId, item.Status, false);
                }
            }
            StatusText = "All active and queued transcriptions cancellation requested.";
            UpdateCommandStates();
        }
        private void ClearQueueInternal()
        {
            StatusText = "Clearing queue...";
            _logger.LogInformation("Clearing transcription queue of non-active items.");

            List<TrialWorld.Presentation.Models.TranscriptionQueueItem> itemsToRemove;
            lock (_activeTranscriptionsLock)
            {
                itemsToRemove = TranscriptionQueue
                    .Where(i => !_activeTranscriptions.ContainsKey(i))
                    .ToList();
            }
            foreach (var item in itemsToRemove)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => TranscriptionQueue.Remove(item));
            }
            StatusText = $"Queue cleared of {itemsToRemove.Count} non-active items.";
            _logger.LogInformation("Queue cleared. Active items remaining: {ActiveCount}", _activeTranscriptions.Count);
        }
        public void StopSingle(TrialWorld.Presentation.Models.TranscriptionQueueItem item)
        {
            if (item == null) return;
            _logger.LogInformation("Attempting to stop transcription for: {FileName}", item.FileName);
            bool wasActive = false;
            lock (_activeTranscriptionsLock)
            {
                if (_activeTranscriptions.TryGetValue(item, out var cts))
                {
                    if (!cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                        StatusText = $"Stopping item: {item.FileName}";
                        _logger.LogInformation("Cancellation token signaled for active item: {FileName}", item.FileName);
                    }
                    else
                    {
                        StatusText = $"Item {item.FileName} already stopping/stopped.";
                    }
                    wasActive = true;
                }
            }
            if (!wasActive && item.CurrentPhase == TranscriptionPhase.Queued)
            {
                 UpdateItemStatusOnUiThread(item, TranscriptionPhase.Cancelled, 0, "Stopped from queue");
                 StatusText = $"Item {item.FileName} stopped from queue before processing.";
            }
            else if (!wasActive)
            {
                StatusText = $"Item {item.FileName} is not actively processing or already completed/failed.";
            }
            UpdateCommandStates();
        }
        public void RemoveSingle(TrialWorld.Presentation.Models.TranscriptionQueueItem item)
        {
            if (item == null) return;
            _logger.LogInformation("Attempting to remove item: {FileName}", item.FileName);
            bool removedFromQueue = false;
            lock (_activeTranscriptionsLock)
            {
                if (_activeTranscriptions.TryGetValue(item, out var cts))
                {
                    if (!cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                        _logger.LogInformation("Requested cancellation for active item {FileName} before removal.", item.FileName);
                        _notificationService.ShowWarning($"Transcription for '{item.FileName}' is being stopped. It will be automatically removed from the active list once processing fully ceases.", "Stopping Active Item");
                    }
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (TranscriptionQueue.Remove(item))
                        {
                            StatusText = $"Removed item: {item.FileName}";
                            _logger.LogInformation("Removed item from queue: {FileName}", item.FileName);
                            removedFromQueue = true;
                        }
                    });
                }
            }
            if(removedFromQueue) UpdateCommandStates();
        }
        #endregion

        #region Search Methods

        public async Task<bool> SetActiveTranscriptionForSearchAsync(string transcriptId)
        {
            if (string.IsNullOrEmpty(transcriptId))
            {
                StatusText = "No transcription ID provided for search.";
                _logger.LogWarning("SetActiveTranscriptionForSearchAsync called with empty transcriptId.");
                _currentTranscriptionForSearch = null;
                _currentTranscriptIdForSearch = string.Empty;
                return false;
            }
            StatusText = $"Loading transcription data for '{transcriptId}' for search...";
            _logger.LogInformation("Attempting to load transcription {TranscriptId} for search.", transcriptId);
            try
            {
                ITranscriptionResult? result = null;
                var getResultMethod = _transcriptionService.GetType().GetMethod("GetTranscriptionResultAsync");
                if (getResultMethod != null && _transcriptionService != null)
                {
                     // The interface does not define GetTranscriptionResultAsync; fetch status as a placeholder.
var status = await _transcriptionService.GetTranscriptionStatusAsync(transcriptId, CancellationToken.None);
// TODO: Implement and call GetFullTranscriptAsync or equivalent to fetch the full transcript object by ID when available.
// result = await _transcriptionService.GetFullTranscriptAsync(transcriptId, CancellationToken.None);
                }
                if (result != null && result.Status == TranscriptionStatus.Completed)
                {
                    _currentTranscriptIdForSearch = transcriptId;
                    _currentTranscriptionForSearch = result;
                    StatusText = $"Loaded transcription from service: {transcriptId}";
                    _logger.LogInformation("Successfully loaded transcription {TranscriptId} from service for search.", transcriptId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Could not load transcription {TranscriptId} from service, or service does not provide full result. Attempting file system via DatabaseLoader.", transcriptId);
                    if (_databaseLoader != null)
                    {
                        var availableTranscripts = await _databaseLoader.GetAvailableTranscriptsAsync();
                        var matchingFile = availableTranscripts.FirstOrDefault(t =>
                            (t.TranscriptId == transcriptId) ||
                            (t.FileName != null && t.FileName.Contains(transcriptId, StringComparison.OrdinalIgnoreCase)) ||
                            (t.FileName != null && Path.GetFileNameWithoutExtension(t.FileName).Equals(transcriptId, StringComparison.OrdinalIgnoreCase)));

                        if (matchingFile != null)
                        {
                            var mediaTranscript = await _databaseLoader.LoadTranscriptFileAsync(matchingFile.FullPath);
                            if (mediaTranscript != null)
                            {
                                _currentTranscriptIdForSearch = transcriptId;
                                _currentTranscriptionForSearch = ConvertToTranscriptionResult(mediaTranscript, transcriptId);
                                StatusText = $"Loaded transcription from file: {matchingFile.FileName}";
                                _logger.LogInformation("Successfully loaded transcription {TranscriptId} from file {FileName} for search.", transcriptId, matchingFile.FileName);
                                return true;
                            }
                        }
                    }
                }
                _logger.LogWarning("No transcription data found for ID: {TranscriptId} via service or file system for search.", transcriptId);
                StatusText = $"No transcription found for ID: {transcriptId}";
                _currentTranscriptionForSearch = null;
                _currentTranscriptIdForSearch = string.Empty;
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transcription for search: {TranscriptId}", transcriptId);
                StatusText = $"Error loading transcription '{transcriptId}': {ex.Message}";
                _currentTranscriptionForSearch = null;
                _currentTranscriptIdForSearch = string.Empty;
                return false;
            }
        }
        private ITranscriptionResult? ConvertToTranscriptionResult(TrialWorld.Core.Models.Transcription.Transcript? transcript, string originalId)
        {
            if (transcript == null) return null;

            var result = new TranscriptionResult 
            {
                Id = transcript.Id ?? originalId,
                TranscriptId = transcript.Id ?? originalId,
                Status = Core.Models.Transcription.TranscriptionStatus.Completed,
                // Don't set Text directly as it's a read-only property
                // Instead set Transcript which Text property reads from
                Transcript = transcript.Text ?? string.Empty,
                Language = "en-US", // Default language
                Success = true
            };
            
            // Set segments if available
            if (transcript.Segments != null)
            {
                result.Segments = transcript.Segments.ToList();
            }
            
            return result;
        }

        public async Task<int> SearchAsync(string searchText, bool includeWords, bool includeSentiment,
                                     bool includeHighlights, bool includeChapters, string sentimentFilter)
        {
            if (string.IsNullOrEmpty(_currentTranscriptIdForSearch) || _currentTranscriptionForSearch == null)
            {
                _logger.LogWarning("SearchAsync called but no active transcription is set for search.");
                _notificationService.ShowWarning("No active transcription selected for search. Please load or select a transcription.", "Search Error");
                System.Windows.Application.Current.Dispatcher.Invoke(() => SearchResults.Clear());
                StatusText = "No active transcription for search.";
                return 0;
            }

            try
            {
                StatusText = "Searching transcription...";
                _logger.LogInformation("Performing search for '{SearchText}' in transcript '{TranscriptId}'", searchText, _currentTranscriptIdForSearch);

                System.Windows.Application.Current.Dispatcher.Invoke(() => SearchResults.Clear());

                if (_currentTranscriptionForSearch == null)
                {
                     _logger.LogError("_currentTranscriptionForSearch is null before calling SearchService.");
                     StatusText = "Error: No transcription loaded for search.";
                     return 0;
                }
                var results = await _searchService.SearchTranscriptionAsync(
                    _currentTranscriptionForSearch,
                    _currentTranscriptIdForSearch,
                    searchText,
                    includeWords,
                    includeSentiment,
                    includeHighlights,
                    includeChapters,
                    sentimentFilter);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var searchResultItem in results)
                    {
                        SearchResults.Add(searchResultItem);
                    }
                });

                if (searchText.Length > 0)
                {
                    StatusText = $"Found {SearchResults.Count} results for '{searchText}'";
                }
                else
                {
                    StatusText = $"Loaded {SearchResults.Count} items from transcription (no search term)";
                }
                _logger.LogInformation("Search completed. Found {ResultCount} results for '{SearchText}'", SearchResults.Count, searchText);
                return SearchResults.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during search for text '{SearchText}' in transcript '{TranscriptId}'", searchText, _currentTranscriptIdForSearch);
                StatusText = $"Search error: {ex.Message}";
                System.Windows.Application.Current.Dispatcher.Invoke(() => SearchResults.Clear());
                return 0;
            }
        }
        public void JumpToTimestampInPlayer(int timestampMs)
        {
            StatusText = $"Jumping to timestamp: {TimeSpan.FromMilliseconds(timestampMs):mm\\:ss}";
            _logger.LogInformation("Request to jump to timestamp: {TimestampMs}ms", timestampMs);
            RequestMediaSeek?.Invoke(this, TimeSpan.FromMilliseconds(timestampMs));
        }
        public event EventHandler<TimeSpan>? RequestMediaSeek;

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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

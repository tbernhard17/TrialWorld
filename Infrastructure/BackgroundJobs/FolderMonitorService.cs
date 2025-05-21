using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models.Configuration;

namespace TrialWorld.Infrastructure.BackgroundJobs
{
    public class FolderMonitorService : BackgroundService
    {
        private readonly ILogger<FolderMonitorService> _logger;
        private readonly IMediaService _mediaService;
        private readonly IMediaIndexingService _indexingService;
        private readonly IMediaContentIndexerService _mediaContentIndexerService;
        private readonly FolderMonitorOptions _options;
        private readonly List<FileSystemWatcher> _watchers = new();
        private readonly HashSet<string> _processingFiles = new();
        private readonly SemaphoreSlim _processingLock = new(1, 1);

        public FolderMonitorService(
            IOptions<FolderMonitorOptions> options,
            IMediaService mediaService,
            IMediaIndexingService indexingService,
            IMediaContentIndexerService mediaContentIndexerService,
            ILogger<FolderMonitorService> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
            _indexingService = indexingService ?? throw new ArgumentNullException(nameof(indexingService));
            _mediaContentIndexerService = mediaContentIndexerService ?? throw new ArgumentNullException(nameof(mediaContentIndexerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Folder monitoring service starting");

            // Register stop notification
            stoppingToken.Register(() => {
                _logger.LogInformation("Folder monitoring service stopping");
                foreach (var watcher in _watchers)
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Dispose();
                }
            });

            // Process any existing files first
            await ScanExistingFilesAsync(stoppingToken);

            // Set up monitoring for new files
            foreach (var folder in _options.MonitoredFolders)
            {
                if (!Directory.Exists(folder))
                {
                    _logger.LogWarning("Monitored folder does not exist: {Folder}", folder);
                    // Create the folder if it doesn't exist
                    try
                    {
                        Directory.CreateDirectory(folder);
                        _logger.LogInformation("Created monitored folder: {Folder}", folder);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create monitored folder: {Folder}", folder);
                        continue;
                    }
                }

                // Set up media file watcher
                var mediaWatcher = new FileSystemWatcher(folder)
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime,
                    IncludeSubdirectories = _options.IncludeSubfolders,
                    EnableRaisingEvents = true
                };

                // Filter for media extensions
                mediaWatcher.Filters.Add("*.mp4");
                mediaWatcher.Filters.Add("*.mov");
                mediaWatcher.Filters.Add("*.avi");
                mediaWatcher.Filters.Add("*.mkv");
                mediaWatcher.Filters.Add("*.mp3");
                mediaWatcher.Filters.Add("*.wav");
                mediaWatcher.Filters.Add("*.m4a");

                mediaWatcher.Created += (sender, e) => OnFileCreated(e.FullPath, stoppingToken);
                _watchers.Add(mediaWatcher);

                _logger.LogInformation("Monitoring folder for new media: {Folder}", folder);
                
                // Set up transcript file watcher
                string transcriptFolder = Path.Combine(folder, "transcripts");
                if (!Directory.Exists(transcriptFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(transcriptFolder);
                        _logger.LogInformation("Created transcript folder: {Folder}", transcriptFolder);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create transcript folder: {Folder}", transcriptFolder);
                        continue;
                    }
                }
                
                var transcriptWatcher = new FileSystemWatcher(transcriptFolder)
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite,
                    IncludeSubdirectories = false,
                    EnableRaisingEvents = true
                };
                
                // Filter for transcript files
                transcriptWatcher.Filters.Add("*.json");
                
                transcriptWatcher.Created += (sender, e) => OnTranscriptFileCreated(e.FullPath, stoppingToken);
                transcriptWatcher.Changed += (sender, e) => OnTranscriptFileChanged(e.FullPath, stoppingToken);
                _watchers.Add(transcriptWatcher);
                
                _logger.LogInformation("Monitoring folder for transcript files: {Folder}", transcriptFolder);
            }

            // Keep the service running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ScanExistingFilesAsync(CancellationToken cancellationToken)
        {
            foreach (var folder in _options.MonitoredFolders)
            {
                if (!Directory.Exists(folder))
                {
                    continue;
                }

                _logger.LogInformation("Scanning existing files in {Folder}", folder);
                
                var mediaExtensions = new[] { ".mp4", ".mov", ".avi", ".mkv", ".mp3", ".wav", ".m4a" };
                var searchOption = _options.IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                
                var existingFiles = Directory.GetFiles(folder, "*.*", searchOption)
                    .Where(f => mediaExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                    .ToList();

                _logger.LogInformation("Found {Count} existing media files in {Folder}", existingFiles.Count, folder);
                
                foreach (var file in existingFiles)
                {
                    // Process files that aren't already in the system
                    await ProcessNewFileAsync(file, cancellationToken);
                }
            }
        }

        private async void OnFileCreated(string filePath, CancellationToken cancellationToken)
        {
            _logger.LogInformation("New file detected: {FilePath}", filePath);

            // File might still be being written to
            await WaitForFileReadyAsync(filePath, cancellationToken);
            
            await ProcessNewFileAsync(filePath, cancellationToken);
        }

        private async Task WaitForFileReadyAsync(string filePath, CancellationToken cancellationToken)
        {
            const int maxRetries = 10;
            const int delayMs = 1000; // 1 second
            
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    // Attempt to open the file for reading to see if it's available
                    using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        // File is available for exclusive access, it's ready
                        break;
                    }
                }
                catch (IOException)
                {
                    // File is still being written to
                    _logger.LogDebug("File {FilePath} is not ready yet. Waiting...", filePath);
                    await Task.Delay(delayMs, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error checking if file is ready: {FilePath}", filePath);
                    // Don't retry on unexpected errors
                    return;
                }
            }
        }

        private async Task ProcessNewFileAsync(string filePath, CancellationToken cancellationToken)
        {
            // Use semaphore to avoid duplicating processing the same file
            await _processingLock.WaitAsync(cancellationToken);
            try
            {
                // Skip if already processing or if cancelled
                if (_processingFiles.Contains(filePath) || cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                _processingFiles.Add(filePath);
            }
            finally
            {
                _processingLock.Release();
            }

            try
            {
                _logger.LogInformation("Processing new file: {FilePath}", filePath);
                
                // Import the media file into the system
                var title = Path.GetFileNameWithoutExtension(filePath);
                var importedMedia = await _mediaService.ImportMediaAsync(filePath, title, null, null, cancellationToken);
                
                if (importedMedia != null && !string.IsNullOrEmpty(importedMedia.Id))
                {
                    _logger.LogInformation("Successfully imported media file: {FilePath} with ID: {MediaId}", filePath, importedMedia.Id);
                    
                    // Queue the file for transcription
                    await _mediaService.ProcessMediaAsync(importedMedia.Id, true, cancellationToken);
                    var processingResult = true;
                    
                    if (processingResult)
                    {
                        _logger.LogInformation("Successfully queued {FilePath} for processing", filePath);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to queue {FilePath} for processing", filePath);
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to import media file: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing new file: {FilePath}", filePath);
            }
            finally
            {
                // Remove from processing set
                await _processingLock.WaitAsync(cancellationToken);
                try
                {
                    _processingFiles.Remove(filePath);
                }
                finally
                {
                    _processingLock.Release();
                }
            }
        }
        
        private async void OnTranscriptFileCreated(string filePath, CancellationToken cancellationToken)
        {
            _logger.LogInformation("New transcript file detected: {FilePath}", filePath);
            
            // Wait for file to be ready
            await WaitForFileReadyAsync(filePath, cancellationToken);
            
            await ProcessTranscriptFileAsync(filePath, cancellationToken);
        }
        
        private async void OnTranscriptFileChanged(string filePath, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Transcript file changed: {FilePath}", filePath);
            
            // Wait for file to be ready
            await WaitForFileReadyAsync(filePath, cancellationToken);
            
            await ProcessTranscriptFileAsync(filePath, cancellationToken);
        }
        
        private async Task ProcessTranscriptFileAsync(string filePath, CancellationToken cancellationToken)
        {
            // Use semaphore to avoid duplicating processing the same file
            await _processingLock.WaitAsync(cancellationToken);
            try
            {
                // Skip if already processing or if cancelled
                if (_processingFiles.Contains(filePath) || cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
                _processingFiles.Add(filePath);
            }
            finally
            {
                _processingLock.Release();
            }
            
            try
            {
                _logger.LogInformation("Processing transcript file: {FilePath}", filePath);
                
                // Extract media ID from filename (assuming format: {mediaId}.json)
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                if (Guid.TryParse(fileName, out Guid mediaId))
                {
                    // Trigger indexing for this media ID
                    _logger.LogInformation("Indexing media content for ID: {MediaId} from transcript file", mediaId);
                    
                    // Use the MediaContentIndexerService to index the content
                    bool indexResult = await _mediaContentIndexerService.IndexMediaContentAsync(mediaId.ToString(), cancellationToken);
                    
                    if (indexResult)
                    {
                        _logger.LogInformation("Successfully indexed media content for ID: {MediaId}", mediaId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to index media content for ID: {MediaId}", mediaId);
                    }
                }
                else
                {
                    _logger.LogWarning("Could not extract valid media ID from transcript filename: {FileName}", fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing transcript file: {FilePath}", filePath);
            }
            finally
            {
                // Remove from processing set
                await _processingLock.WaitAsync(cancellationToken);
                try
                {
                    _processingFiles.Remove(filePath);
                }
                finally
                {
                    _processingLock.Release();
                }
            }
        }
    }
}

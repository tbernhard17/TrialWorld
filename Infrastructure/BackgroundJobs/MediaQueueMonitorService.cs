using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Interfaces.Persistence;
using TrialWorld.Core.Models.Processing;
using TrialWorld.Infrastructure.Models.FFmpeg; // For FFmpegOptions
using TrialWorld.Core.Interfaces.Services; // Changed from Core.Services
using TrialWorld.Core.Models; // Added for JobStatus
using TrialWorld.Core.Media.Interfaces; // Added for MediaProcessingOptions

namespace TrialWorld.Infrastructure.BackgroundJobs
{
    /// <summary>
    /// Monitors the media repository for queued items and initiates processing via BackgroundTaskManager.
    /// </summary>
    public class MediaQueueMonitorService : BackgroundService
    {
        private readonly ILogger<MediaQueueMonitorService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IBackgroundTaskManager _backgroundTaskManager; // Use interface
        private readonly FFmpegOptions _options;

        public MediaQueueMonitorService(
            ILogger<MediaQueueMonitorService> logger,
            IServiceScopeFactory scopeFactory,
            IBackgroundTaskManager backgroundTaskManager, // Inject interface
            IOptions<FFmpegOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _backgroundTaskManager = backgroundTaskManager ?? throw new ArgumentNullException(nameof(backgroundTaskManager));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger.LogInformation("MediaQueueMonitorService created.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MediaQueueMonitorService is starting.");

            stoppingToken.Register(() => 
                _logger.LogInformation("MediaQueueMonitorService background task is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("MediaQueueMonitorService checking for queued items...");

                try
                {
                    // Create a scope to resolve scoped services like the repository
                    using var scope = _scopeFactory.CreateScope();
                    var mediaRepository = scope.ServiceProvider.GetRequiredService<IMediaRepository>();

                    // Find items that are currently queued
                    var queuedItems = await mediaRepository.GetByStatusAsync(MediaProcessingStatus.Queued, stoppingToken);

                    if (!queuedItems.Any())
                    {
                        _logger.LogInformation("No queued items found. Waiting...");
                    }
                    else
                    {
                        _logger.LogInformation("Found {Count} queued items. Processing...", queuedItems.Count);
                    }

                    foreach (var mediaItem in queuedItems)
                    {
                        if (stoppingToken.IsCancellationRequested) break; // Check cancellation before processing each item

                        _logger.LogInformation("Attempting to process media item: {MediaId}", mediaItem.Id);
                        
                        // Try to claim the item by setting its status to Processing
                        bool claimed = await mediaRepository.UpdateProcessingStatusAsync(mediaItem.Id, MediaProcessingStatus.Processing, stoppingToken);

                        if (claimed)
                        {
                            _logger.LogInformation("Claimed media item {MediaId} for processing.", mediaItem.Id);

                            // Define the work to be done by the BackgroundTaskManager
                            Func<CancellationToken, Task> workItem = async (cancellationTaskToken) =>
                            {
                                // Combine external cancellation with BackgroundTaskManager's token
                                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, cancellationTaskToken);
                                var combinedToken = linkedCts.Token;

                                // Create a new scope for this specific task execution
                                using var taskScope = _scopeFactory.CreateScope();
                                var pipeline = taskScope.ServiceProvider.GetRequiredService<IMediaProcessingPipeline>();
                                var taskRepo = taskScope.ServiceProvider.GetRequiredService<IMediaRepository>();
                                var logger = taskScope.ServiceProvider.GetRequiredService<ILogger<MediaQueueMonitorService>>(); // Get logger for task context

                                MediaProcessingStatus finalStatus = MediaProcessingStatus.Failed; // Default to Failed
                                try
                                {
                                    logger.LogInformation("Starting pipeline for {MediaId}", mediaItem.Id);

                                    // TODO: Determine options based on MediaMetadata or default?
                                    // Assuming default options for now, may need refinement
                                    var options = new MediaProcessingOptions 
                                    {
                                        GenerateThumbnails = true, // Example
                                        Transcribe = true         // Example
                                    };
                                    
                                    // Initialize and start the pipeline
                                    bool initialized = await pipeline.InitializeAsync(mediaItem.FilePath, options, combinedToken);
                                    if (initialized)
                                    {
                                        // TODO: How to handle progress reporting from here?
                                        await pipeline.StartAsync(null, combinedToken); // Pass null progress for now

                                        // Assuming StartAsync completes when done (needs verification based on pipeline impl)
                                        var pipelineStatus = await pipeline.GetStatusAsync(); 
                                        finalStatus = pipelineStatus == MediaProcessingStatus.Completed ? MediaProcessingStatus.Completed : MediaProcessingStatus.Failed;
                                        logger.LogInformation("Pipeline finished for {MediaId} with status: {Status}", mediaItem.Id, finalStatus);
                                    }
                                    else
                                    {
                                        logger.LogError("Pipeline initialization failed for {MediaId}", mediaItem.Id);
                                        finalStatus = MediaProcessingStatus.Failed;
                                    }
                                }
                                catch (OperationCanceledException) when (combinedToken.IsCancellationRequested)
                                {
                                    logger.LogWarning("Processing cancelled for {MediaId}", mediaItem.Id);
                                    finalStatus = MediaProcessingStatus.Cancelled; // Or Failed?
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, "Error processing media item {MediaId} in background task.", mediaItem.Id);
                                    finalStatus = MediaProcessingStatus.Failed;
                                }
                                finally
                                {
                                    // Always attempt to update the final status
                                    try
                                    {
                                        await taskRepo.UpdateProcessingStatusAsync(mediaItem.Id, finalStatus, CancellationToken.None); // Use CancellationToken.None as status update is critical
                                        logger.LogInformation("Updated final status for {MediaId} to {Status}", mediaItem.Id, finalStatus);
                                    }
                                    catch(Exception updateEx)
                                    {
                                         logger.LogError(updateEx, "Failed to update final status for {MediaId} to {Status}", mediaItem.Id, finalStatus);
                                    }
                                }
                            };

                            // Queue the work item
                            await _backgroundTaskManager.QueueTaskAsync(mediaItem.Id, workItem);
                            _logger.LogInformation("Queued background task for media item {MediaId}", mediaItem.Id);
                        }
                        else
                        {
                            // Failed to claim - likely another instance picked it up.
                            _logger.LogWarning("Failed to claim media item {MediaId} (already processing?). Skipping.", mediaItem.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during media queue processing loop.");
                }

                // Wait for the configured interval
                var pollingInterval = TimeSpan.FromSeconds(_options.QueuePollingIntervalSeconds);
                await Task.Delay(pollingInterval, stoppingToken);
            }

            _logger.LogInformation("MediaQueueMonitorService background task is stopping.");
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MediaQueueMonitorService is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
} 
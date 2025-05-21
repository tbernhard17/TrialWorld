using System.Threading.Tasks;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models.Processing;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Media.Interfaces;
using System;
using System.Threading;
using System.IO;
using System.Linq;
using TrialWorld.Core.Models.Progress;
using TrialWorld.Core.Models.Transcription;

namespace TrialWorld.Infrastructure.MediaIngestion.Services
{
    /// <summary>
    /// Production-ready implementation for the media processing pipeline.
    /// Implements both stateful pipeline and direct processing.
    /// </summary>
    public class MediaProcessingPipeline : IMediaProcessingPipeline, IDirectMediaProcessor
    {
        private readonly ILogger<MediaProcessingPipeline> _logger;
        private readonly IMediaIndexingService _indexingService;
        private readonly ITranscriptionService _transcriptionService;
        private readonly IThumbnailExtractor _thumbnailExtractor;
        private readonly IMediaService _mediaService;
        private MediaProcessingStatus _currentStatus = MediaProcessingStatus.Unknown;
        private Guid _currentMediaId;
        private string _currentInputPath = string.Empty;

        public MediaProcessingPipeline(
            ILogger<MediaProcessingPipeline> logger,
            IMediaIndexingService indexingService,
            ITranscriptionService transcriptionService,
            IThumbnailExtractor thumbnailExtractor,
            IMediaService mediaService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _indexingService = indexingService ?? throw new ArgumentNullException(nameof(indexingService));
            _transcriptionService = transcriptionService ?? throw new ArgumentNullException(nameof(transcriptionService));
            _thumbnailExtractor = thumbnailExtractor ?? throw new ArgumentNullException(nameof(thumbnailExtractor));
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
        }

        public async Task<bool> InitializeAsync(string inputPath, MediaProcessingOptions options, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Initializing pipeline for {InputPath}", inputPath);
                
                // Store for later use
                _currentInputPath = inputPath;
                
                // Validate the input file exists
                if (!File.Exists(inputPath))
                {
                    _logger.LogError("Input file {InputPath} does not exist", inputPath);
                    _currentStatus = MediaProcessingStatus.Failed;
                    return false;
                }
                
                // Try to find the media by ID if we have one in the filename
                string fileName = Path.GetFileName(inputPath);
                try {
                    // Check for media records with similar names
                    var mediaList = await _mediaService.GetMediaListAsync(string.Empty, 1, 100, cancellationToken);
                    var matchingMedia = mediaList.FirstOrDefault(m => 
                        !string.IsNullOrEmpty(m.FilePath) && 
                        Path.GetFileName(m.FilePath).Equals(fileName, StringComparison.OrdinalIgnoreCase));
                        
                    if (matchingMedia != null)
                    {
                        _currentMediaId = Guid.Parse(matchingMedia.Id);
                        _logger.LogInformation("Found existing media record with ID {MediaId} for {InputPath}", _currentMediaId, inputPath);
                    }
                    else
                    {
                        _logger.LogWarning("No existing media record found for {InputPath}", inputPath);
                        // We'll import the media
                        var importedMedia = await _mediaService.ImportMediaAsync(inputPath, Path.GetFileNameWithoutExtension(inputPath), null, null, cancellationToken);
                        if (importedMedia != null)
                        {
                            _currentMediaId = Guid.Parse(importedMedia.Id);
                            _logger.LogInformation("Imported media with ID {MediaId} for {InputPath}", _currentMediaId, inputPath);
                        }
                        else
                        {
                            _currentMediaId = Guid.Empty;
                            _logger.LogWarning("Failed to import media for {InputPath}", inputPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error looking up media ID for {InputPath}", inputPath);
                    _currentMediaId = Guid.Empty;
                }
                
                _currentStatus = MediaProcessingStatus.Queued;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize pipeline for {InputPath}", inputPath);
                _currentStatus = MediaProcessingStatus.Failed;
                return false;
            }
        }

        public async Task StartAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting pipeline processing for {InputPath}", _currentInputPath);
                _currentStatus = MediaProcessingStatus.Processing;
                
                // Report initial progress
                progress?.Report(0.1);
                
                // Step 1: Extract thumbnails
                _logger.LogInformation("Extracting thumbnails for {InputPath}", _currentInputPath);
                try
                {
                    // Generate a thumbnail output path
                    string thumbnailDir = Path.Combine(Path.GetDirectoryName(_currentInputPath) ?? string.Empty, "thumbnails");
                    Directory.CreateDirectory(thumbnailDir);
                    string thumbnailPath = Path.Combine(thumbnailDir, $"{Path.GetFileNameWithoutExtension(_currentInputPath)}_thumb.jpg");
                    
                    await _thumbnailExtractor.ExtractThumbnailAsync(_currentInputPath, thumbnailPath, TimeSpan.FromSeconds(5), cancellationToken);
                    _logger.LogInformation("Thumbnail extracted successfully to {ThumbnailPath}", thumbnailPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Thumbnail extraction failed for {InputPath}", _currentInputPath);
                    // Continue despite thumbnail failure
                }
                
                progress?.Report(0.3);
                
                // Step 2: Transcribe media
                _logger.LogInformation("Transcribing media {InputPath}", _currentInputPath);
                
                // Create progress adapter for transcription
                var transcriptionProgress = new Progress<Core.Models.Transcription.TranscriptionProgressUpdate>(p => {
                    // Scale transcription progress from 30-80%
                    double progressValue = p.OverallProgress;
                    double scaledProgress = 0.3 + (progressValue * 0.5);
                    progress?.Report(Math.Min(0.8, scaledProgress));
                });
                
                // Convert from Guid to string ID
                string mediaId = _currentMediaId.ToString();
                
                // Initiate media processing with transcription enabled
                if (_currentMediaId != Guid.Empty)
                {
                    try
                    {
                        // Create a transcription config
                        var config = new Core.Models.Transcription.TranscriptionConfig
                        {
                            EnableSpeakerDiarization = true,
                            EnableSentimentAnalysis = true,
                            EnableAutoHighlights = true,
                            EnableEntityDetection = true
                        };
                        
                        // Directly use the transcription service for better control
                        var result = await _transcriptionService.TranscribeWithProgressAsync(
                            _currentInputPath,
                            config,
                            transcriptionProgress,
                            cancellationToken);
                        
                        if (result == null || !result.Success)
                        {
                            _logger.LogError("Transcription failed for {InputPath}: {Error}",
                                _currentInputPath, result?.Error ?? "Unknown error");
                            _currentStatus = MediaProcessingStatus.Failed;
                            progress?.Report(1.0); // Complete with failure
                            return;
                        }
                        
                        // Save the transcript to the media service
                        var transcript = new Core.Models.MediaTranscript
                        {
                            MediaId = mediaId,
                            FullText = result.Transcript, // Changed Text to FullText
                            Language = result.Language,
                            // Convert TranscriptSegment to MediaTranscriptSegment
                            Segments = (result.Segments ?? new List<TrialWorld.Core.Models.Transcription.TranscriptSegment>()).Select(s => new TrialWorld.Core.Models.Transcription.TranscriptSegment {
                                Text = s.Text,
                                StartTime = s.StartTime,
                                EndTime = s.EndTime,
                                Confidence = s.Confidence,
                                Speaker = s.Speaker,
                                Sentiment = s.Sentiment,
                                Words = s.Words
                            }).ToList(),
                            CreatedDate = DateTime.UtcNow, // Changed CreatedAt to CreatedDate
                            Id = result.TranscriptId // Changed TranscriptId to Id
                        };
                        
                        bool saveResult = await _mediaService.SaveMediaTranscriptAsync(mediaId, transcript, cancellationToken);
                        if (!saveResult)
                        {
                            _logger.LogWarning("Failed to save transcript for media ID {MediaId}", mediaId);
                            // Continue despite save failure
                        }
                        
                        // Update the media record to indicate it has been transcribed
                        await _mediaService.UpdateMediaAsync(mediaId, new Core.Models.MediaUpdateData
                        {
                            IsTranscribed = true
                            // Removed ProcessingStatus property
                        }, cancellationToken);
                        
                        _logger.LogInformation("Transcription completed successfully for media ID {MediaId}", mediaId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during transcription for {InputPath}", _currentInputPath);
                        _currentStatus = MediaProcessingStatus.Failed;
                        progress?.Report(1.0); // Complete with failure
                        return;
                    }
                }
                else
                {
                    _logger.LogWarning("Cannot initiate transcription without a valid media ID");
                }
                
                progress?.Report(0.8);
                
                // Step 3: Index the media (only if we have a valid media ID)
                if (_currentMediaId != Guid.Empty)
                {
                    _logger.LogInformation("Indexing media {MediaId}", _currentMediaId);
                    progress?.Report(0.85); // Update progress to 85%
                    
                    try
                    {
                        var indexingResult = await _indexingService.ProcessAndIndexMediaAsync(_currentMediaId, cancellationToken);
                        
                        if (!indexingResult)
                        {
                            _logger.LogWarning("Indexing failed for media {MediaId}", _currentMediaId);
                            // Continue despite indexing failure, but log it
                        }
                        else
                        {
                            _logger.LogInformation("Successfully indexed media {MediaId}", _currentMediaId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during indexing for media {MediaId}", _currentMediaId);
                        // Continue despite indexing error
                    }
                }
                else
                {
                    _logger.LogWarning("Skipping indexing due to unknown media ID for {InputPath}", _currentInputPath);
                }
                
                progress?.Report(0.95); // Update progress to 95%
                
                // Mark as complete
                _currentStatus = MediaProcessingStatus.Completed;
                progress?.Report(1.0);
                _logger.LogInformation("Pipeline processing finished successfully for {InputPath}", _currentInputPath);
            }
            catch (OperationCanceledException)
            {
                _currentStatus = MediaProcessingStatus.Cancelled;
                _logger.LogInformation("Pipeline processing was cancelled for {InputPath}", _currentInputPath);
                throw;
            }
            catch (Exception ex)
            {
                _currentStatus = MediaProcessingStatus.Failed;
                _logger.LogError(ex, "Pipeline processing failed for {InputPath}", _currentInputPath);
                progress?.Report(1.0); // Complete with failure
            }
        }

        public Task<MediaProcessingStatus> GetStatusAsync()
        {
            return Task.FromResult(_currentStatus);
        }

        // Keep original method? Depends if it's still needed alongside the stateful methods.
        // If kept, it should likely represent a one-shot processing call.
        public async Task<MediaProcessingResult> ProcessMediaAsync(string inputPath, ProcessingOptions options, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Processing media {InputPath} via pipeline (one-shot)", inputPath);
            
            // Create a default MediaProcessingOptions object
            var pipelineOptions = new MediaProcessingOptions();
            
            // Initialize the pipeline
            bool initialized = await InitializeAsync(inputPath, pipelineOptions, cancellationToken);
            if (!initialized)
            {
                return new MediaProcessingResult 
                { 
                    Success = false, 
                    ErrorMessage = "Pipeline initialization failed",
                    ProcessedFilePath = inputPath 
                };
            }
            
            // Start processing
            await StartAsync(null, cancellationToken);
            
            // Get final status
            var finalStatus = await GetStatusAsync();
            
            return new MediaProcessingResult 
            { 
                Success = (finalStatus == MediaProcessingStatus.Completed), 
                ProcessedFilePath = inputPath,
                ErrorMessage = finalStatus != MediaProcessingStatus.Completed ? $"Processing finished with status: {finalStatus}" : string.Empty
            };
        }

        // --- IDirectMediaProcessor Implementation ---
        async Task<MediaProcessingResult> IDirectMediaProcessor.ProcessAsync(
            string inputPath, 
            IProgress<WorkflowStageProgress>? progress,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing directly via IDirectMediaProcessor for {InputPath}", inputPath);
            progress?.Report(new WorkflowStageProgress { FileIdentifier = inputPath, StageName = "Initialization", ProgressPercentage = 0.0 });
            
            // Use the stateful methods internally for the direct call
            // TODO: Determine appropriate MediaProcessingOptions for direct processing
            bool initialized = await InitializeAsync(inputPath, new MediaProcessingOptions(), cancellationToken);
            if (!initialized)
            {
                progress?.Report(new WorkflowStageProgress { FileIdentifier = inputPath, StageName = "Failed Initialization", ProgressPercentage = 1.0, IsError = true, ErrorMessage = "Pipeline initialization failed." });
                return new MediaProcessingResult { Success = false, ErrorMessage = "Pipeline initialization failed.", ProcessedFilePath = inputPath };
            }

            progress?.Report(new WorkflowStageProgress { FileIdentifier = inputPath, StageName = "Processing", ProgressPercentage = 0.1 });
            
            // Simple progress adapter if needed
            IProgress<double>? pipelineProgress = null;
            if (progress != null)
            {
                pipelineProgress = new Progress<double>(p => progress.Report(new WorkflowStageProgress { FileIdentifier = inputPath, StageName = "Processing", ProgressPercentage = p }));
            }

            await StartAsync(pipelineProgress, cancellationToken);
            var finalStatus = await GetStatusAsync();

            progress?.Report(new WorkflowStageProgress { 
                FileIdentifier = inputPath, 
                StageName = finalStatus == MediaProcessingStatus.Completed ? "Complete" : "Failed", 
                ProgressPercentage = 1.0, 
                IsComplete = (finalStatus == MediaProcessingStatus.Completed),
                IsError = (finalStatus != MediaProcessingStatus.Completed),
                ErrorMessage = finalStatus != MediaProcessingStatus.Completed ? $"Processing finished with status: {finalStatus}" : null
            });

            return new MediaProcessingResult 
            { 
                Success = (finalStatus == MediaProcessingStatus.Completed), 
                ProcessedFilePath = inputPath, 
                ErrorMessage = finalStatus != MediaProcessingStatus.Completed ? $"Processing finished with status: {finalStatus}" : string.Empty
            }; 
        }
    }
} 
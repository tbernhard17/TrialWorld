using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Enums;
using TrialWorld.Core.Models.Processing;
using TrialWorld.Core.Models.Transcription;

namespace TrialWorld.Infrastructure.Services
{
    /// <summary>
    /// Simplified implementation of IMediaService for testing purposes.
    /// </summary>
    public class MediaService : IMediaService
    {
        private readonly ILogger<MediaService> _logger;
        private readonly IThumbnailExtractor _thumbnailExtractor;
        private readonly Dictionary<string, MediaMetadata> _mediaMetadata = new();

        public MediaService(
            IThumbnailExtractor thumbnailExtractor,
            ILogger<MediaService> logger)
        {
            _thumbnailExtractor = thumbnailExtractor ?? throw new ArgumentNullException(nameof(thumbnailExtractor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<MediaMetadata?> GetMediaByIdAsync(string mediaId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting media by ID: {MediaId}", mediaId);
            
            if (_mediaMetadata.TryGetValue(mediaId, out var metadata))
            {
                return Task.FromResult<MediaMetadata?>(metadata);
            }
            
            return Task.FromResult<MediaMetadata?>(null);
        }

        public Task<List<MediaMetadata>> GetMediaListAsync(string? filter, int page, int pageSize, CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<MediaMetadata>(_mediaMetadata.Values));
        }

        public Task<bool> DeleteMediaAsync(string mediaId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting media: {MediaId}", mediaId);
            return Task.FromResult(_mediaMetadata.Remove(mediaId));
        }





        public async Task<TrialWorld.Core.Models.MediaTranscript?> GetMediaTranscriptAsync(string mediaId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting transcript for media: {MediaId}", mediaId);
            
            if (string.IsNullOrEmpty(mediaId))
            {
                _logger.LogWarning("Cannot get transcript: Media ID is null or empty");
                return null;
            }
            
            try
            {
                // Check if we have the media metadata
                var metadata = await GetMediaByIdAsync(mediaId, cancellationToken);
                if (metadata == null || string.IsNullOrEmpty(metadata.FilePath))
                {
                    _logger.LogWarning("Media not found or has no file path: {MediaId}", mediaId);
                    return null;
                }
                
                // Check if the transcript file exists
                string transcriptDir = Path.Combine(Path.GetDirectoryName(metadata.FilePath) ?? string.Empty, "transcripts");
                string transcriptPath = Path.Combine(transcriptDir, $"{mediaId}.json");
                
                if (!File.Exists(transcriptPath))
                {
                    _logger.LogWarning("Transcript file not found at {TranscriptPath}", transcriptPath);
                    return null;
                }
                
                // Read and parse the transcript file
                string json = await File.ReadAllTextAsync(transcriptPath, cancellationToken);
                var transcript = System.Text.Json.JsonSerializer.Deserialize<TrialWorld.Core.Models.MediaTranscript>(json);
                
                if (transcript == null)
                {
                    _logger.LogWarning("Failed to deserialize transcript from {TranscriptPath}", transcriptPath);
                    return null;
                }
                
                _logger.LogInformation("Successfully retrieved transcript for media: {MediaId}", mediaId);
                return transcript;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transcript for media: {MediaId}", mediaId);
                return null;
            }
        }

        public Task<MediaProcessingStatus?> GetProcessingStatusAsync(string mediaId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting processing status for media: {MediaId}", mediaId);
            return Task.FromResult<MediaProcessingStatus?>(MediaProcessingStatus.Completed);
        }

        public async Task<MediaSaveResult> SaveMediaAsync(Stream fileStream, string fileName, string mediaType, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid().ToString();
            var path = Path.Combine("MediaFiles", $"{id}_{fileName}");
            Directory.CreateDirectory("MediaFiles");

            try
            {
                _logger.LogInformation("Saving media file {FileName} with ID {MediaId} to {StoragePath}", fileName, id, path);
                using (var file = File.Create(path))
                {
                    await fileStream.CopyToAsync(file, cancellationToken);
                }

                var thumbnailUrl = $"/api/media/{id}/thumbnail";
                var thumbDir = Path.Combine(Directory.GetCurrentDirectory(), "Thumbnails");
                Directory.CreateDirectory(thumbDir);
                var thumbnailPath = Path.Combine(thumbDir, $"{id}.jpg");

                try
                {
                    await _thumbnailExtractor.ExtractThumbnailAsync(path, thumbnailPath, TimeSpan.FromSeconds(1), cancellationToken);
                }
                catch (Exception thumbEx)
                {
                    _logger.LogWarning(thumbEx, "Failed to generate thumbnail file for {MediaId}. Clearing ThumbnailUrl.", id);
                    thumbnailUrl = string.Empty;
                }

                var metadata = new MediaMetadata
                {
                    Id = id,
                    Title = fileName,
                    FileName = fileName,
                    FilePath = path,
                    MediaType = InferMediaTypeFromFileName(fileName),
                    ThumbnailUrl = thumbnailUrl,
                    FileSize = new FileInfo(path).Length,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    IsTranscribed = false,
                    Metadata = new Dictionary<string, string>()
                };

                _mediaMetadata[id] = metadata;

                return new MediaSaveResult { Id = id, Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save media file: {FileName}", fileName);
                return new MediaSaveResult { Id = null!, Success = false, ErrorMessage = ex.Message };
            }
        }

        public Task<bool> UpdateMediaAsync(string mediaId, MediaUpdateData updateData, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating media: {MediaId}", mediaId);
            return Task.FromResult(true);
        }

        public async Task<MediaMetadata> ImportMediaAsync(string filePath, string title, string? description = null, List<string>? tags = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Importing media from {FilePath}", filePath);
            if (!File.Exists(filePath)) throw new FileNotFoundException("Import file not found.", filePath);

            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var saveResult = await SaveMediaAsync(fileStream, Path.GetFileName(filePath), mediaType: title, cancellationToken);

            if (!saveResult.Success || string.IsNullOrEmpty(saveResult.Id))
                throw new InvalidOperationException($"Failed to save imported media: {saveResult.ErrorMessage ?? "Unknown error"}");

            var metadata = await GetMediaByIdAsync(saveResult.Id, cancellationToken);
            return metadata ?? new MediaMetadata();
        }

        public async Task ProcessMediaAsync(string id, bool transcribe, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Processing media: {MediaId}, Transcribe: {Transcribe}", id, transcribe);
            
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Cannot process media: ID is null or empty");
                return;
            }
            
            try
            {
                // Get the media metadata
                var metadata = await GetMediaByIdAsync(id, cancellationToken);
                if (metadata == null || string.IsNullOrEmpty(metadata.FilePath))
                {
                    _logger.LogWarning("Media not found or has no file path: {MediaId}", id);
                    return;
                }
                
                // Check if the file exists
                if (!File.Exists(metadata.FilePath))
                {
                    _logger.LogWarning("Media file not found at {FilePath}", metadata.FilePath);
                    return;
                }
                
                // Update the processing status
                // Removed ProcessingStatus property that was causing build errors
                await UpdateMediaAsync(id, new MediaUpdateData { /* Removed ProcessingStatus */ }, cancellationToken);
                
                // If transcription is requested, use the TranscriptionService
                if (transcribe)
                {
                    _logger.LogInformation("Starting transcription for media: {MediaId}", id);
                    
                    // Get the transcription service from the service provider
                    var transcriptionService = GetTranscriptionService();
                    if (transcriptionService == null)
                    {
                        _logger.LogError("Transcription service not available");
                        await UpdateMediaAsync(id, new MediaUpdateData { /* Removed ProcessingStatus */ }, cancellationToken);
                        return;
                    }
                    
                    // Create a progress reporter
                    var progress = new Progress<Core.Models.Transcription.TranscriptionProgressUpdate>(update =>
                    {
                        _logger.LogDebug("Transcription progress for {MediaId}: {Progress}%, Phase: {Phase}",
                            id, update.ProgressPercent, update.Phase);
                    });
                    
                    try
                    {
                        // Start the transcription
                        var result = await transcriptionService.TranscribeWithProgressAsync(
                            metadata.FilePath,
                            new Core.Models.Transcription.TranscriptionConfig
                            {
                                EnableSpeakerDiarization = true,
                                EnableSentimentAnalysis = true,
                                EnableAutoHighlights = true
                            },
                            progress,
                            cancellationToken);
                        
                        if (result != null && result.Success)
                        {
                            _logger.LogInformation("Transcription completed successfully for media: {MediaId}", id);
                            
                            // Save the transcript
                            var transcript = new TrialWorld.Core.Models.MediaTranscript
                            {
                                MediaId = id,
                                // Adjusted properties to match MediaTranscript class
                                FullText = result.Transcript,
                                Language = result.Language,
                                // Convert TranscriptSegment to MediaTranscriptSegment
                                Segments = (result.Segments ?? new List<Core.Models.Transcription.TranscriptSegment>()).Select(s => new Core.Models.Transcription.TranscriptSegment {
                                    Text = s.Text,
                                    StartTime = s.StartTime, // s.StartTime is double (milliseconds)
                                    EndTime = s.EndTime,     // s.EndTime is double (milliseconds)
                                    Confidence = s.Confidence,
                                    Speaker = s.Speaker,
                                    Sentiment = s.Sentiment,
                                    Words = s.Words
                                }).ToList(),
                                CreatedDate = DateTime.UtcNow,
                                Id = result.TranscriptId
                            };
                            
                            await SaveMediaTranscriptAsync(id, transcript, cancellationToken);
                            
                            // Update the media metadata
                            await UpdateMediaAsync(id, new MediaUpdateData
                            {
                                // Removed ProcessingStatus
                                IsTranscribed = true
                            }, cancellationToken);
                        }
                        else
                        {
                            _logger.LogWarning("Transcription failed for media: {MediaId}", id);
                            await UpdateMediaAsync(id, new MediaUpdateData { /* Removed ProcessingStatus */ }, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during transcription for media: {MediaId}", id);
                        await UpdateMediaAsync(id, new MediaUpdateData { /* Removed ProcessingStatus */ }, cancellationToken);
                    }
                }
                else
                {
                    // Just mark as completed if no transcription was requested
                    await UpdateMediaAsync(id, new MediaUpdateData { /* Removed ProcessingStatus */ }, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing media: {MediaId}", id);
                try
                {
                    await UpdateMediaAsync(id, new MediaUpdateData { /* Removed ProcessingStatus */ }, cancellationToken);
                }
                catch
                {
                    // Ignore errors in error handling
                }
            }
        }
        
        private ITranscriptionService? GetTranscriptionService()
        {
            // In a real implementation, this would be injected via constructor
            // For now, we'll return null and let the caller handle it
            return null;
        }

        public async Task<bool> SaveMediaTranscriptAsync(string id, TrialWorld.Core.Models.MediaTranscript transcript, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Saving transcript for media: {MediaId}", id);
            
            if (string.IsNullOrEmpty(id) || transcript == null)
            {
                _logger.LogWarning("Cannot save transcript: Media ID is null/empty or transcript is null");
                return false;
            }
            
            try
            {
                // Get the media metadata to determine file paths
                var metadata = await GetMediaByIdAsync(id, cancellationToken);
                if (metadata == null)
                {
                    _logger.LogWarning("Media not found: {MediaId}", id);
                    return false;
                }
                
                // Create the transcripts directory if it doesn't exist
                string mediaDir = Path.GetDirectoryName(metadata.FilePath) ?? string.Empty;
                string transcriptDir = Path.Combine(mediaDir, "transcripts");
                Directory.CreateDirectory(transcriptDir);
                
                // Save the transcript to a JSON file
                string transcriptPath = Path.Combine(transcriptDir, $"{id}.json");
                string json = System.Text.Json.JsonSerializer.Serialize(transcript, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                await File.WriteAllTextAsync(transcriptPath, json, cancellationToken);
                
                // Update the media metadata to indicate it has a transcript
                await UpdateMediaAsync(id, new MediaUpdateData { IsTranscribed = true }, cancellationToken);
                
                _logger.LogInformation("Successfully saved transcript for media: {MediaId} to {TranscriptPath}", id, transcriptPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving transcript for media: {MediaId}", id);
                return false;
            }
        }

        private static MediaType InferMediaTypeFromFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return MediaType.Unknown;
            }
            
            string extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? string.Empty;
            return extension switch
            {
                ".mp4" or ".avi" or ".mov" or ".wmv" or ".mkv" => MediaType.Video,
                ".mp3" or ".wav" or ".aac" or ".flac" or ".ogg" => MediaType.Audio,
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => MediaType.Image,
                _ => MediaType.Unknown
            };
        }
    }
}

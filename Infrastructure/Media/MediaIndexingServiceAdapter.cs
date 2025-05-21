using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Search;

namespace TrialWorld.Infrastructure.Media
{
    /// <summary>
    /// Production-ready implementation of IMediaIndexingService that works with the AssemblyAI API implementation.
    /// Provides real metadata extraction and transcript indexing.
    /// </summary>
    public class MediaIndexingServiceAdapter : IMediaIndexingService
    {
        private readonly ISearchIndexer _indexer;
        private readonly IMediaService _mediaService;
        private readonly IContentAnalysisService _contentAnalysisService;
        private readonly ILogger<MediaIndexingServiceAdapter> _logger;

        public MediaIndexingServiceAdapter(
            ISearchIndexer indexer,
            IMediaService mediaService,
            IContentAnalysisService contentAnalysisService,
            ILogger<MediaIndexingServiceAdapter> logger)
        {
            _indexer = indexer ?? throw new ArgumentNullException(nameof(indexer));
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
            _contentAnalysisService = contentAnalysisService ?? throw new ArgumentNullException(nameof(contentAnalysisService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Processes a media file and indexes it.
        /// </summary>
        /// <param name="mediaId">The unique identifier of the media item.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if indexing was successful, false otherwise.</returns>
        public async Task<bool> ProcessAndIndexMediaAsync(Guid mediaId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Processing and indexing media with ID: {MediaId}", mediaId);
            
            try
            {
                string mediaIdStr = mediaId.ToString();
                
                // Get media metadata
                var metadata = await _mediaService.GetMediaByIdAsync(mediaIdStr, cancellationToken);
                if (metadata == null)
                {
                    _logger.LogWarning("Media not found for ID: {MediaId}", mediaId);
                    return false;
                }
                
                // Get transcript
                var transcript = await _mediaService.GetMediaTranscriptAsync(mediaIdStr, cancellationToken);
                
                // Create searchable content object
                var content = new SearchableContent
                {
                    Id = mediaIdStr,
                    MediaId = mediaIdStr,
                    Title = metadata.Title ?? System.IO.Path.GetFileNameWithoutExtension(metadata.FileName),
                    Description = metadata.Title ?? string.Empty, // Using Title as fallback since Description doesn't exist
                    Transcript = transcript?.FullText ?? string.Empty, // Using FullText instead of Text
                    ThumbnailUrl = metadata.ThumbnailUrl,
                    CreatedAt = metadata.CreatedDate,
                    Duration = metadata.Duration, // Removed null coalescing operator
                    Topics = new List<string>(),
                    Timestamps = new List<SearchableTimestamp>(),
                    Metadata = new Dictionary<string, string>
                    {
                        { "ProcessedBy", "AssemblyAI" },
                        { "IndexedAt", DateTime.UtcNow.ToString("o") },
                        { "MediaType", metadata.MediaType.ToString() }
                    }
                };
                
                // Add any additional metadata
                if (metadata.Metadata != null)
                {
                    foreach (var kvp in metadata.Metadata)
                    {
                        content.Metadata[kvp.Key] = kvp.Value;
                    }
                }
                
                // Extract topics if transcript is available and file exists
                if (transcript != null && !string.IsNullOrEmpty(metadata.FilePath) && System.IO.File.Exists(metadata.FilePath))
                {
                    try
                    {
                        var topics = await _contentAnalysisService.ExtractTopicsAsync(metadata.FilePath, 10, cancellationToken);
                        if (topics != null && topics.Count > 0)
                        {
                            content.Topics = topics.Select(t => t.Name).ToList();
                            _logger.LogInformation("Extracted {Count} topics for media: {MediaId}", topics.Count, mediaId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to extract topics for media: {MediaId}", mediaId);
                        // Continue without topics
                    }
                }
                
                // Add transcript segments as timestamps
                if (transcript != null && transcript.Segments != null && transcript.Segments.Count > 0)
                {
                    content.Timestamps = transcript.Segments
                        .Where(s => !string.IsNullOrEmpty(s.Text))
                        .Select(s => new SearchableTimestamp
                        {
                            // Only set Text once
                            Text = s.Text,
                            // Convert milliseconds to TimeSpan for StartTime and EndTime
                            Start = TimeSpan.FromMilliseconds(s.StartTime),
                            End = TimeSpan.FromMilliseconds(s.EndTime)
                        })
                        .ToList();
                    
                    _logger.LogInformation("Added {Count} transcript segments as timestamps for media: {MediaId}",
                        content.Timestamps.Count, mediaId);
                }
                
                // Index the content
                var result = await _indexer.IndexAsync(mediaIdStr, content, cancellationToken);
                
                if (result)
                {
                    _logger.LogInformation("Successfully indexed media: {MediaId}", mediaId);
                    
                    // Update media metadata to indicate it's been indexed
                    await _mediaService.UpdateMediaAsync(mediaIdStr, new MediaUpdateData
                    {
                        Metadata = new Dictionary<string, string>
                        {
                            { "Indexed", "true" },
                            { "IndexedAt", DateTime.UtcNow.ToString("o") }
                        }
                    }, cancellationToken);
                }
                else
                {
                    _logger.LogWarning("Failed to index media: {MediaId}", mediaId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing and indexing media: {MediaId}", mediaId);
                return false;
            }
        }
    }
}
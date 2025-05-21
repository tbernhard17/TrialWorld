using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Search;

namespace TrialWorld.Infrastructure.MediaIngestion.Services
{
    /// <summary>
    /// Service responsible for indexing media content for search functionality.
    /// Implements IMediaContentIndexerService to provide production-ready indexing capabilities.
    /// </summary>
    public class MediaContentIndexerService : IMediaContentIndexerService
    {
        private readonly IMediaService _mediaService;
        private readonly ISearchIndexer _searchIndexer;
        private readonly IContentAnalysisService _contentAnalysisService;
        private readonly ILogger<MediaContentIndexerService> _logger;

        /// <summary>
        /// Initializes a new instance of the MediaContentIndexerService class.
        /// </summary>
        /// <param name="mediaService">The media service for retrieving media data.</param>
        /// <param name="searchIndexer">The search indexer for indexing content.</param>
        /// <param name="contentAnalysisService">The content analysis service for extracting metadata.</param>
        /// <param name="logger">The logger.</param>
        public MediaContentIndexerService(
            IMediaService mediaService,
            ISearchIndexer searchIndexer,
            IContentAnalysisService contentAnalysisService,
            ILogger<MediaContentIndexerService> logger)
        {
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
            _searchIndexer = searchIndexer ?? throw new ArgumentNullException(nameof(searchIndexer));
            _contentAnalysisService = contentAnalysisService ?? throw new ArgumentNullException(nameof(contentAnalysisService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Indexes media content for a specific media ID.
        /// </summary>
        /// <param name="mediaId">The ID of the media to index.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if indexing was successful, false otherwise.</returns>
        public async Task<bool> IndexMediaContentAsync(string mediaId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(mediaId))
            {
                _logger.LogError("Cannot index media content: Media ID is null or empty");
                return false;
            }

            try
            {
                _logger.LogInformation("Starting indexing of media content for ID: {MediaId}", mediaId);

                // Get media metadata
                var metadata = await _mediaService.GetMediaByIdAsync(mediaId, cancellationToken);
                if (metadata == null)
                {
                    _logger.LogWarning("Media not found for ID: {MediaId}", mediaId);
                    return false;
                }

                // Get transcript
                var transcript = await _mediaService.GetMediaTranscriptAsync(mediaId, cancellationToken);
                if (transcript == null)
                {
                    _logger.LogWarning("No transcript found for media ID: {MediaId}", mediaId);
                    // Continue indexing without transcript
                }

                // Create searchable content
                var searchableContent = new SearchableContent
                {
                    Id = mediaId,
                    MediaId = mediaId,
                    Title = metadata.Title ?? System.IO.Path.GetFileNameWithoutExtension(metadata.FileName),
                    Description = metadata.Title ?? string.Empty, // Using Title as fallback since Notes doesn't exist
                    Transcript = transcript?.FullText ?? string.Empty, // Using FullText instead of Text
                    ThumbnailUrl = metadata.ThumbnailUrl,
                    CreatedAt = metadata.CreatedDate,
                    Duration = metadata.Duration, // Removed null coalescing operator
                    Metadata = new Dictionary<string, string>(),
                    Topics = new List<string>(),
                    Timestamps = new List<SearchableTimestamp>()
                };

                // Add metadata
                if (metadata.Metadata != null)
                {
                    foreach (var kvp in metadata.Metadata)
                    {
                        searchableContent.Metadata[kvp.Key] = kvp.Value;
                    }
                }

                // Add media type
                searchableContent.Metadata["MediaType"] = metadata.MediaType.ToString();

                // Extract topics if transcript is available and file exists
                if (transcript != null && !string.IsNullOrEmpty(metadata.FilePath) && System.IO.File.Exists(metadata.FilePath))
                {
                    try
                    {
                        var topics = await _contentAnalysisService.ExtractTopicsAsync(metadata.FilePath, 10, cancellationToken);
                        if (topics != null && topics.Count > 0)
                        {
                            searchableContent.Topics = topics.Select(t => t.Name).ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to extract topics for media ID: {MediaId}", mediaId);
                        // Continue without topics
                    }
                }

                // Add transcript segments as timestamps
                if (transcript != null && transcript.Segments != null && transcript.Segments.Count > 0)
                {
                    searchableContent.Timestamps = transcript.Segments
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
                }

                // Index the content
                var result = await _searchIndexer.IndexAsync(mediaId, searchableContent, cancellationToken);
                
                if (result)
                {
                    _logger.LogInformation("Successfully indexed media content for ID: {MediaId}", mediaId);
                }
                else
                {
                    _logger.LogWarning("Failed to index media content for ID: {MediaId}", mediaId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing media content for ID: {MediaId}", mediaId);
                return false;
            }
        }

        /// <summary>
        /// Processes and indexes media content.
        /// </summary>
        /// <param name="mediaId">The ID of the media to process and index.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if processing and indexing was successful, false otherwise.</returns>
        public async Task<bool> ProcessAndIndexMediaAsync(Guid mediaId, CancellationToken cancellationToken = default)
        {
            return await IndexMediaContentAsync(mediaId.ToString(), cancellationToken);
        }
    }
}
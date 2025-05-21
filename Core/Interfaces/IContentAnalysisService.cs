using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Provides advanced content analysis functionality for media files.
    /// </summary>
    public interface IContentAnalysisService
    {
        /// <summary>
        /// Identifies high-interest segments in audio/video content.
        /// </summary>
        /// <param name="mediaFilePath">Path to the media file.</param>
        /// <param name="topics">Optional list of topics to look for.</param>
        /// <param name="progress">Optional progress reporter.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>List of highlight segments with timestamps and confidence scores.</returns>
        Task<List<ContentHighlight>> IdentifyHighlightsAsync(
            string mediaFilePath,
            List<string>? topics = null,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Suggests optimal frame timestamps for thumbnails based on content analysis.
        /// </summary>
        /// <param name="mediaFilePath">Path to the media file.</param>
        /// <param name="maxResults">Maximum number of suggestions to return.</param>
        /// <param name="progress">Optional progress reporter.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>List of recommended frame timestamps for thumbnails with reasons.</returns>
        Task<List<ThumbnailSuggestion>> SuggestThumbnailTimestampsAsync(
            string mediaFilePath,
            int maxResults = 3,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a content summary with chapters and key points.
        /// </summary>
        /// <param name="mediaFilePath">Path to the media file.</param>
        /// <param name="maxChapters">Maximum number of chapters to generate.</param>
        /// <param name="progress">Optional progress reporter.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Content summary with chapters and key points.</returns>
        Task<ContentSummary> GenerateContentSummaryAsync(
            string mediaFilePath,
            int maxChapters = 5,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Extracts searchable topics and keywords from content.
        /// </summary>
        /// <param name="mediaFilePath">Path to the media file.</param>
        /// <param name="maxResults">Maximum number of topics to extract.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>List of topics and keywords with confidence scores.</returns>
        Task<List<ContentTopic>> ExtractTopicsAsync(
            string mediaFilePath,
            int maxResults = 10,
            CancellationToken cancellationToken = default);
    }
}
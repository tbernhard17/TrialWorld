using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Search;
using TrialWorld.Core.Models.Transcription;
using TrialWorld.Core.Models.Analysis;

namespace TrialWorld.Core.Interfaces

{
    /// <summary>
    /// Provides search capabilities across media content and transcripts.
    /// </summary>
    public interface ISearchService
    {
        /// <summary>
        /// Gets the current processing status of the search database.
        /// </summary>
        /// <returns>The current processing status.</returns>
        Task<SearchProcessingStatus> GetProcessingStatusAsync();
        
        /// <summary>
        /// Event that is raised when the processing status changes.
        /// </summary>
        event EventHandler<SearchProcessingStatus> ProcessingStatusChanged;
        #region Gesture Types

        /// <summary>
        /// Standard gesture type for pointing.
        /// </summary>
        public const string GesturePointing = "pointing";

        /// <summary>
        /// Standard gesture type for waving.
        /// </summary>
        public const string GestureWaving = "waving";

        /// <summary>
        /// Standard gesture type for laughing.
        /// </summary>
        public const string GestureLaughing = "laughing";

        /// <summary>
        /// Standard gesture type for yelling.
        /// </summary>
        public const string GestureYelling = "yelling";

        /// <summary>
        /// Standard gesture type for flailing arms.
        /// </summary>
        public const string GestureFlailing = "flailing";

        /// <summary>
        /// Standard gesture type for standing.
        /// </summary>
        public const string GestureStanding = "standing";

        /// <summary>
        /// Standard gesture type for sitting.
        /// </summary>
        public const string GestureSitting = "sitting";

        /// <summary>
        /// Standard gesture type for raising hand.
        /// </summary>
        public const string GestureRaisingHand = "raising_hand";

        /// <summary>
        /// Standard gesture type for clapping.
        /// </summary>
        public const string GestureClapping = "clapping";

        /// <summary>
        /// Standard gesture type for nodding head.
        /// </summary>
        public const string GestureHeadNodding = "head_nodding";

        /// <summary>
        /// Standard gesture type for shaking head.
        /// </summary>
        public const string GestureHeadShaking = "head_shaking";

        #endregion

        /// <summary>
        /// Performs advanced search using structured query object
        /// </summary>
        /// <param name="query">Structured search query</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Search results</returns>
        Task<SearchResults> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs semantic search to find media content related to a concept.
        /// </summary>
        /// <param name="concept">The concept to search for.</param>
        /// <param name="filters">Optional search filters to apply.</param>
        /// <param name="pageNumber">Page number for pagination.</param>
        /// <param name="pageSize">Number of results per page.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Collection of search results semantically matching the concept.</returns>
        Task<SearchResults> SemanticSearchAsync(
            string concept,
            SearchFilters? filters = default,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches for media with multiple combined criteria for detailed content analysis.
        /// </summary>
        /// <param name="criteria">Combined search criteria including text, gestures, emotions, and speakers.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Collection of search results matching the combined criteria.</returns>
        Task<SearchResults> AdvancedSearchAsync(
            AdvancedSearchCriteria criteria,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches for media by a specific speaker.
        /// </summary>
        /// <param name="speakerName">Name of the speaker to search for.</param>
        /// <param name="queryText">Optional text to search alongside speaker filter.</param>
        /// <param name="filters">Optional search filters to apply.</param>
        /// <param name="pageNumber">Page number for pagination.</param>
        /// <param name="pageSize">Number of results per page.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Collection of search results with the specified speaker.</returns>
        Task<SearchResults> SearchBySpeakerAsync(
            string speakerName,
            string? queryText = null,
            SearchFilters? filters = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets suggestions for query completion.
        /// </summary>
        /// <param name="partialQuery">Partial query text to get suggestions for.</param>
        /// <param name="maxSuggestions">Maximum number of suggestions to return.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>List of query suggestions.</returns>
        Task<IList<string>> GetQuerySuggestionsAsync(
            string partialQuery,
            int maxSuggestions = 5,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Builds or updates the search index.
        /// </summary>
        /// <param name="mediaDirectories">Directories containing media to index.</param>
        /// <param name="incremental">Whether to perform an incremental update instead of full rebuild.</param>
        /// <param name="progress">Optional progress reporter.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Summary of the indexing operation.</returns>
        Task<IndexingResult> BuildIndexAsync(
            IList<string> mediaDirectories,
            bool incremental = true,
            IProgress<IndexingProgress>? progress = default,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets statistics about the current search index.
        /// </summary>
        /// <returns>Information about the search index.</returns>
        Task<IndexStatistics> GetIndexStatisticsAsync();

        /// <summary>
        /// Indexes a media file and its associated metadata/analysis results
        /// </summary>
        /// <param name="mediaId">The media identifier</param>
        /// <param name="mediaMetadata">The basic media information</param>
        /// <param name="transcription">Transcription results (optional)</param>
        /// <param name="contentAnalysis">Content analysis results (optional)</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous indexing operation.</returns>
        Task<bool> IndexMediaAsync(
            string mediaId,
            MediaMetadata mediaMetadata,
            TranscriptionResult? transcription = null,
            ContentAnalysisResult? contentAnalysis = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a media file in the index using MediaMetadata
        /// </summary>
        /// <param name="mediaId">The media identifier</param>
        /// <param name="mediaMetadata">Updated media metadata</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>True if update was successful</returns>
        Task<bool> UpdateMediaAsync(
            string mediaId,
            MediaMetadata mediaMetadata,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a media item from the index. (Used by MediaController's DeleteMediaAsync)
        /// </summary>
        /// <param name="mediaId">Media identifier</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>True if successfully removed</returns>
        Task<bool> RemoveFromIndexAsync(string mediaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clears the entire search index
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>True if successfully cleared</returns>
        Task<bool> ClearIndexAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates user search preferences.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="preferences">User preferences.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if successful.</returns>
        Task<bool> UpdateUserPreferencesAsync(string userId, UserPreferences preferences, CancellationToken cancellationToken = default);

        /// <summary>
        /// Indexes textual content associated with a media file.
        /// </summary>
        /// <param name="mediaId">The media identifier</param>
        /// <param name="content">The content of the media</param>
        /// <param name="duration">The duration of the media</param>
        /// <param name="timestamps">The timestamps of the media</param>
        /// <param name="topics">The topics of the media</param>
        /// <param name="segments">The segments of the media</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>True if indexing was successful, false otherwise.</returns>
        Task<bool> IndexContentAsync(
            string mediaId,
            string content,
            TimeSpan duration,
            IEnumerable<SearchableTimestamp> timestamps,
            IEnumerable<ContentTopic> topics,
            IEnumerable<ContentSegment> segments,
            CancellationToken cancellationToken = default);
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Options for enhanced search.
    /// </summary>
    public class EnhancedSearchOptions
    {
        /// <summary>
        /// Gets or sets the maximum number of results to return.
        /// </summary>
        public int MaxResults { get; init; } = 10;

        /// <summary>
        /// Gets or sets the minimum match confidence threshold.
        /// </summary>
        public double MinConfidence { get; init; } = 0.7;

        /// <summary>
        /// Gets or sets optional face IDs to filter results by.
        /// </summary>
        public string[]? FaceFilter { get; init; }

        /// <summary>
        /// Gets or sets whether to include emotion analysis in results.
        /// </summary>
        public bool IncludeEmotions { get; init; } = true;

        /// <summary>
        /// Gets or sets whether to include transcript snippets in results.
        /// </summary>
        public bool IncludeTranscripts { get; init; } = true;

        /// <summary>
        /// Gets or sets the context window size around transcript matches in seconds.
        /// </summary>
        public double TranscriptContextWindow { get; init; } = 5.0;
    }

    /// <summary>
    /// Individual search result item.
    /// </summary>
    public class EnhancedSearchResult
    {
        /// <summary>
        /// Gets or sets the path to the media file.
        /// </summary>
        public required string MediaPath { get; init; }

        /// <summary>
        /// Gets or sets the match confidence score.
        /// </summary>
        public double Confidence { get; init; }

        /// <summary>
        /// Gets or sets the timestamp of the match.
        /// </summary>
        public TimeSpan Timestamp { get; init; }

        /// <summary>
        /// Gets or sets the transcript snippet around the match.
        /// </summary>
        public string? TranscriptSnippet { get; init; }

        /// <summary>
        /// Gets or sets emotional spikes in the media.
        /// </summary>
        public List<EmotionalSpike>? EmotionalSpikes { get; init; }

        /// <summary>
        /// Gets or sets face matches in the media.
        /// </summary>
        public List<FaceMatch>? FaceMatches { get; init; }

        /// <summary>
        /// Gets or sets the paths to thumbnails for this result.
        /// </summary>
        public List<string> ThumbnailPaths { get; init; } = new();
    }

    /// <summary>
    /// Emotional spike detected in media.
    /// </summary>
    public class EmotionalSpike
    {
        /// <summary>
        /// Gets or sets the type of emotion detected.
        /// </summary>
        public required string Emotion { get; init; }

        /// <summary>
        /// Gets or sets the intensity of the emotion.
        /// </summary>
        public double Intensity { get; init; }

        /// <summary>
        /// Gets or sets the timestamp of the emotional spike.
        /// </summary>
        public TimeSpan Timestamp { get; init; }
    }

    /// <summary>
    /// Face match detected in media.
    /// </summary>
    public class FaceMatch
    {
        /// <summary>
        /// Gets or sets the face identifier.
        /// </summary>
        public required string FaceId { get; init; }

        /// <summary>
        /// Gets or sets the match confidence.
        /// </summary>
        public double Confidence { get; init; }

        /// <summary>
        /// Gets or sets the timestamp of the face match.
        /// </summary>
        public TimeSpan Timestamp { get; init; }
    }

    /// <summary>
    /// User feedback on search results.
    /// </summary>
    public class EnhancedSearchFeedback
    {
        /// <summary>
        /// Gets or sets the ID of the result being rated.
        /// </summary>
        public required string ResultId { get; init; }

        /// <summary>
        /// Gets or sets the original query that produced this result.
        /// </summary>
        public required string Query { get; init; }

        /// <summary>
        /// Gets or sets whether the result was relevant to the query.
        /// </summary>
        public bool IsRelevant { get; init; }

        /// <summary>
        /// Gets or sets the user's rating from 1-5.
        /// </summary>
        public int Rating { get; init; }
    }

    /// <summary>
    /// Statistics about the search index.
    /// </summary>
    public class EnhancedSearchIndexStats
    {
        /// <summary>
        /// Gets or sets the number of files in the index.
        /// </summary>
        public int IndexedFiles { get; init; }

        /// <summary>
        /// Gets or sets the total duration of all indexed media.
        /// </summary>
        public TimeSpan TotalDuration { get; init; }

        /// <summary>
        /// Gets or sets the number of unique faces detected.
        /// </summary>
        public int UniqueFaces { get; init; }

        /// <summary>
        /// Gets or sets the number of emotional spikes detected.
        /// </summary>
        public int EmotionalSpikes { get; init; }

        /// <summary>
        /// Gets or sets the size of the index in bytes.
        /// </summary>
        public long IndexSize { get; init; }
    }

    /// <summary>
    /// Interface for search engine.
    /// </summary>
    public interface ISearchEngine
    {
        Task<IList<string>> GetAutocompleteSuggestions(string partialQuery, int maxSuggestions = 10, CancellationToken cancellationToken = default);
    }
}
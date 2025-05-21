using System;
using System.Collections.Generic;

namespace TrialWorld.Core.Models.Search
{
    /// <summary>
    /// Collection of search results with metadata.
    /// </summary>
    public class SearchResults
    {
        /// <summary>
        /// Individual search result items.
        /// </summary>
        public required IList<SearchResultItem> Items { get; set; } = new List<SearchResultItem>();

        /// <summary>
        /// Total number of items matching the query (may be more than returned).
        /// </summary>
        public int TotalMatches { get; set; }

        /// <summary>
        /// How long the search took to execute, in milliseconds.
        /// </summary>
        public long ElapsedMilliseconds { get; set; }
    }

    /// <summary>
    /// Single search result item with match information.
    /// </summary>
    public class SearchResultItem
    {
        /// <summary>
        /// Unique identifier for this result.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Path to the media file.
        /// </summary>
        public required string MediaPath { get; set; }

        /// <summary>
        /// Title or filename of the media.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Relevance score for this result (0.0-1.0).
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Type of media (video, audio, etc.).
        /// </summary>
        public required string MediaType { get; set; }

        /// <summary>
        /// Duration of the media in seconds.
        /// </summary>
        public double DurationInSeconds { get; set; }

        /// <summary>
        /// Date when the media was created or modified.
        /// </summary>
        public DateTime FileDate { get; set; }

        /// <summary>
        /// Size of the file in bytes.
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Key timestamps where the query terms match.
        /// </summary>
        public required IList<MatchTimestamp> MatchTimestamps { get; set; } = new List<MatchTimestamp>();

        /// <summary>
        /// Snippets of transcript text that match the query.
        /// </summary>
        public required IList<TextMatch> TextMatches { get; set; } = new List<TextMatch>();

        /// <summary>
        /// List of identified speakers in this media.
        /// </summary>
        public required IList<SpeakerSummary> Speakers { get; set; } = new List<SpeakerSummary>();

        /// <summary>
        /// Key topics or themes in this media.
        /// </summary>
        public required IList<string> Topics { get; set; } = new List<string>();
    }

    /// <summary>
    /// A timestamp where a search match occurs.
    /// </summary>
    public class MatchTimestamp
    {
        /// <summary>
        /// Time in seconds from the start of the media.
        /// </summary>
        public double TimeInSeconds { get; set; }

        /// <summary>
        /// Confidence score for this match (0.0-1.0).
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Type of match (transcript, face, emotion, etc.).
        /// </summary>
        public required string MatchType { get; set; }
    }

    /// <summary>
    /// A text match within a transcript.
    /// </summary>
    public class TextMatch
    {
        /// <summary>
        /// Matched text snippet with highlighting markers.
        /// </summary>
        public required string HighlightedText { get; set; }

        /// <summary>
        /// Start time of this text segment in seconds.
        /// </summary>
        public double StartTime { get; set; }

        /// <summary>
        /// End time of this text segment in seconds.
        /// </summary>
        public double EndTime { get; set; }

        /// <summary>
        /// Speaker for this text segment, if available.
        /// </summary>
        public string? Speaker { get; set; }
    }

    /// <summary>
    /// Summary information about a speaker in media.
    /// </summary>
    public class SpeakerSummary
    {
        /// <summary>
        /// Speaker label (e.g., "Speaker A", "John Doe").
        /// </summary>
        public required string Label { get; set; }

        /// <summary>
        /// Total time this speaker spoke in seconds.
        /// </summary>
        public double TotalSpeakingTime { get; set; }

        /// <summary>
        /// Number of transcript segments attributed to this speaker.
        /// </summary>
        public int SegmentCount { get; set; }

        /// <summary>
        /// Timestamp of the first appearance in seconds.
        /// </summary>
        public double FirstAppearance { get; set; }

        /// <summary>
        /// Confidence score for speaker identification (0.0-1.0).
        /// </summary>
        public double Confidence { get; set; }
    }

    /// <summary>
    /// Represents a facet (category) for filtering search results.
    /// </summary>
    public class SearchFacet
    {
        /// <summary>
        /// Name of the facet (e.g., "Speaker", "MediaType").
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Dictionary of facet values and their counts.
        /// </summary>
        public required Dictionary<string, int> Values { get; set; } = new();
    }

    /// <summary>
    /// Represents criteria for an advanced search operation.
    /// </summary>
    public class AdvancedSearchCriteria
    {
        /// <summary>
        /// The main search query string. Can be null or empty for filter-only searches.
        /// </summary>
        public string? Query { get; set; }

        /// <summary>
        /// Filters to apply to the search.
        /// </summary>
        public SearchFilters? Filters { get; set; }

        /// <summary>
        /// Maximum number of results to return.
        /// </summary>
        public int MaxResults { get; set; } = 50;

        /// <summary>
        /// Number of results to skip (for pagination).
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        /// Field to sort the results by.
        /// </summary>
        public string? SortField { get; set; }

        /// <summary>
        /// Whether to sort in ascending order. Defaults to false (descending).
        /// </summary>
        public bool SortAscending { get; set; }

        /// <summary>
        /// Whether to include facet information in the results.
        /// </summary>
        public bool IncludeFacets { get; set; } = true;

        /// <summary>
        /// Whether to include related query suggestions in the results.
        /// </summary>
        public bool IncludeRelatedQueries { get; set; } = true;
    }

    /// <summary>
    /// Represents filters that can be applied to a search query.
    /// </summary>
    public class SearchFilters
    {
        public SearchFilters()
        {
            MediaTypes = new HashSet<string>();
            Speakers = new HashSet<string>();
            Topics = new HashSet<string>();
            Keywords = new HashSet<string>();
        }

        /// <summary>
        /// Filter by date range (e.g., creation or modification date).
        /// </summary>
        public DateTimeRange? DateRange { get; set; }

        /// <summary>
        /// Filter by media duration range.
        /// </summary>
        public TimeSpanRange? DurationRange { get; set; }

        /// <summary>
        /// Filter by media types (e.g., "video", "audio").
        /// </summary>
        public HashSet<string>? MediaTypes { get; set; }

        /// <summary>
        /// Filter by identified speakers.
        /// </summary>
        public HashSet<string>? Speakers { get; set; }

        /// <summary>
        /// Filter by topics or themes.
        /// </summary>
        public HashSet<string>? Topics { get; set; }

        /// <summary>
        /// Filter by specific keywords found in the content.
        /// </summary>
        public HashSet<string>? Keywords { get; set; }

        /// <summary>
        /// Minimum confidence score for matches (e.g., for keyword relevance).
        /// </summary>
        public double? MinConfidence { get; set; }

        /// <summary>
        /// Checks if any filter criteria are set.
        /// </summary>
        public bool HasAnyFilter =>
            DateRange != null ||
            DurationRange != null ||
            (MediaTypes?.Count ?? 0) > 0 ||
            (Speakers?.Count ?? 0) > 0 ||
            (Topics?.Count ?? 0) > 0 ||
            (Keywords?.Count ?? 0) > 0 ||
            MinConfidence.HasValue;
    }

    /// <summary>
    /// Represents progress during an indexing operation.
    /// </summary>
    public class IndexingProgress
    {
        /// <summary>
        /// Total number of files to be processed.
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// Number of files processed successfully so far.
        /// </summary>
        public int ProcessedFiles { get; set; }

        /// <summary>
        /// Number of files that failed to process.
        /// </summary>
        public int FailedFiles { get; set; }

        /// <summary>
        /// Path of the file currently being processed.
        /// </summary>
        public string? CurrentFile { get; set; }

        /// <summary>
        /// Description of the current operation (e.g., "Extracting metadata", "Indexing content").
        /// </summary>
        public string? CurrentOperation { get; set; }

        /// <summary>
        /// Overall progress percentage.
        /// </summary>
        public double ProgressPercentage => TotalFiles > 0 ? (ProcessedFiles * 100.0) / TotalFiles : 0;

        /// <summary>
        /// Indicates if the indexing process is complete.
        /// </summary>
        public bool IsComplete => ProcessedFiles + FailedFiles >= TotalFiles;
    }

    /// <summary>
    /// Represents the result of an indexing operation.
    /// </summary>
    public class IndexingResult
    {
        /// <summary>
        /// Whether indexing was successful overall.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Total number of files processed.
        /// </summary>
        public int ProcessedFiles { get; set; }

        /// <summary>
        /// Number of files that failed processing.
        /// </summary>
        public int FailedFiles { get; set; }

        /// <summary>
        /// Total processing time.
        /// </summary>
        public TimeSpan ProcessingTime { get; set; }

        /// <summary>
        /// List of files that failed processing.
        /// </summary>
        public required IList<string> FailedFilePaths { get; set; } = new List<string>();

        /// <summary>
        /// Error messages for failed files.
        /// </summary>
        public required Dictionary<string, string> ErrorMessages { get; set; } = new();
    }

    /// <summary>
    /// Represents a search query with filters and sorting options
    /// </summary>
    public class SearchQuery
    {
        /// <summary>
        /// Search text to match
        /// </summary>
        public required string Text { get; set; }

        /// <summary>
        /// Filters to apply to the search
        /// </summary>
        public required Dictionary<string, string> Filters { get; set; } = new();

        /// <summary>
        /// Field to sort by
        /// </summary>
        public required string SortBy { get; set; }

        /// <summary>
        /// Number of results per page
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Current page number
        /// </summary>
        public int PageNumber { get; set; } = 1;
    }

    /// <summary>
    /// Represents basic statistics about the search index.
    /// </summary>
    public class IndexStatistics
    {
        /// <summary>
        /// Total number of documents in the index.
        /// </summary>
        public int TotalDocuments { get; set; }

        /// <summary>
        /// Total size of the index on disk in bytes.
        /// </summary>
        public long TotalSizeBytes { get; set; }

        /// <summary>
        /// Last modification timestamp of the index (UTC).
        /// </summary>
        public DateTime LastModifiedUtc { get; set; }

        /// <summary>
        /// Current status of the index (e.g., "Ready", "Indexing", "Error").
        /// </summary>
        public string IndexStatus { get; set; } = "Unknown";

        /// <summary>
        /// Indicates whether the search database is currently processing files.
        /// </summary>
        public bool IsProcessing { get; set; }

        /// <summary>
        /// The current operation being performed (e.g., "Indexing", "Optimizing", "Rebuilding").
        /// </summary>
        public string CurrentOperation { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the media file currently being processed, if applicable.
        /// </summary>
        public string? CurrentMediaId { get; set; }

        /// <summary>
        /// The name of the file currently being processed, if applicable.
        /// </summary>
        public string? CurrentFileName { get; set; }

        /// <summary>
        /// The estimated completion percentage of the current operation (0-100).
        /// </summary>
        public double ProgressPercentage { get; set; }

        /// <summary>
        /// The number of files processed in the current operation.
        /// </summary>
        public int FilesProcessed { get; set; }

        /// <summary>
        /// The total number of files to process in the current operation.
        /// </summary>
        public int TotalFilesToProcess { get; set; }

        /// <summary>
        /// The estimated time remaining for the current operation, if available.
        /// </summary>
        public TimeSpan? EstimatedTimeRemaining { get; set; }

        /// <summary>
        /// Additional implementation-specific statistics.
        /// </summary>
        public Dictionary<string, string> AdditionalStats { get; set; } = new Dictionary<string, string>();
    }
}
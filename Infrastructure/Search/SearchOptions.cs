using System.Collections.Generic;
using System.IO;

namespace TrialWorld.Infrastructure.Search
{
    /// <summary>
    /// Configuration options for the search service.
    /// </summary>
    public class SearchOptions
    {
        /// <summary>
        /// Gets or sets the path to the search index.
        /// </summary>
        public string IndexPath { get; set; } = "SearchIndex";

        /// <summary>
        /// Gets or sets the maximum number of results to return by default.
        /// </summary>
        public int DefaultMaxResults { get; set; } = 50;

        /// <summary>
        /// Gets or sets the default number of results to return (alias for DefaultMaxResults).
        /// </summary>
        public int DefaultResultsLimit => DefaultMaxResults;

        /// <summary>
        /// Gets or sets whether to use in-memory search index.
        /// </summary>
        public bool UseInMemoryIndex { get; set; } = true;

        /// <summary>
        /// Gets or sets the default media file extensions to index.
        /// </summary>
        public List<string> MediaExtensions { get; set; } = new List<string>
        {
            ".mp4", ".mov", ".avi", ".mkv",
            ".mp3", ".wav", ".m4a"
        };

        /// <summary>
        /// Gets or sets whether to automatically update the index when files change.
        /// </summary>
        public bool AutoUpdateIndex { get; set; } = false;

        /// <summary>
        /// Gets or sets the interval in minutes to check for file changes.
        /// </summary>
        public int AutoUpdateIntervalMinutes { get; set; } = 60;

        /// <summary>
        /// Gets or sets whether to include private directories in the search.
        /// </summary>
        public bool IncludePrivateDirectories { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to perform full text transcription for non-text media.
        /// </summary>
        public bool EnableTranscription { get; set; } = true;

        /// <summary>
        /// Gets or sets the directory where the search index is stored.
        /// </summary>
        public string IndexDirectory { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "SearchIndex");

        /// <summary>
        /// Gets or sets whether to build the search index on startup.
        /// </summary>
        public bool BuildIndexOnStartup { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to use incremental indexing (faster) vs. full rebuilds.
        /// </summary>
        public bool UseIncrementalIndexing { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of documents to process in a batch during indexing.
        /// </summary>
        public int IndexBatchSize { get; set; } = 100;
    }
}
using System;
using System.Collections.Generic;
using System.IO;

namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Represents media content that can be indexed and searched
    /// </summary>
    public class SearchableContent
    {
        /// <summary>
        /// Unique identifier for the searchable content
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Media identifier that this content is associated with
        /// </summary>
        public string MediaId { get; set; } = string.Empty;

        /// <summary>
        /// Title of the media content
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Description of the media content
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// URL to the thumbnail image
        /// </summary>
        public string ThumbnailUrl { get; set; } = string.Empty;

        /// <summary>
        /// Full transcript text
        /// </summary>
        public string Transcript { get; set; } = string.Empty;

        /// <summary>
        /// List of tags associated with the content
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Transcript timestamps with specific segments of text
        /// </summary>
        public List<SearchableTimestamp> Timestamps { get; set; } = new List<SearchableTimestamp>();

        /// <summary>
        /// Additional metadata as key-value pairs
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Creation date of the content
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Last modification date of the content
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.Now;

        /// <summary>
        /// Duration of the media content
        /// </summary>
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Timestamp when the content was recorded
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Search relevance score (populated during search)
        /// </summary>
        public double Score { get; set; } = 0.0;

        /// <summary>
        /// Snippets of searchable content for preview
        /// </summary>
        public List<SearchSnippet> Snippets { get; set; } = new List<SearchSnippet>();

        /// <summary>
        /// Gets the full path to the media file
        /// </summary>
        public string FilePath => Path.GetFullPath(MediaId);

        /// <summary>
        /// Gets the file name from the media ID
        /// </summary>
        public string FileName => Path.GetFileName(MediaId);

        /// <summary>
        /// List of topics associated with the content
        /// </summary>
        public List<string> Topics { get; set; } = new List<string>();

        /// <summary>
        /// Converts a Core model to a SearchableContent instance
        /// </summary>
        public static SearchableContent FromCore(Core.Models.SearchableContent coreContent)
        {
            return new SearchableContent
            {
                Id = coreContent.Id,
                MediaId = coreContent.MediaId,
                Title = coreContent.Title,
                Description = coreContent.Description,
                ThumbnailUrl = coreContent.ThumbnailUrl,
                Transcript = coreContent.Transcript,
                Tags = coreContent.Tags,
                Timestamps = coreContent.Timestamps,
                Metadata = coreContent.Metadata,
                CreatedAt = coreContent.CreatedAt,
                LastModified = coreContent.LastModified,
                Duration = coreContent.Duration,
                Timestamp = coreContent.Timestamp,
                Score = coreContent.Score,
                Snippets = coreContent.Snippets,
                Topics = coreContent.Topics
            };
        }

        /// <summary>
        /// Converts a SearchableContent instance to a Core model
        /// </summary>
        public Core.Models.SearchableContent ToCore()
        {
            return new Core.Models.SearchableContent
            {
                Id = this.Id,
                MediaId = this.MediaId,
                Title = this.Title,
                Description = this.Description,
                ThumbnailUrl = this.ThumbnailUrl,
                Transcript = this.Transcript,
                Tags = this.Tags,
                Timestamps = this.Timestamps,
                Metadata = this.Metadata,
                CreatedAt = this.CreatedAt,
                LastModified = this.LastModified,
                Duration = this.Duration,
                Timestamp = this.Timestamp,
                Score = this.Score,
                Snippets = this.Snippets,
                Topics = this.Topics
            };
        }
    }

    /// <summary>
    /// Represents a snippet of searchable content for preview, extending SearchableTimestamp
    /// </summary>
    public class SearchSnippet : SearchableTimestamp
    {
        /// <summary>
        /// Speaker name associated with this snippet
        /// </summary>
        public string Speaker { get; set; } = string.Empty;

        /// <summary>
        /// Creates a new instance of SearchSnippet
        /// </summary>
        public SearchSnippet() : base()
        {
            // Constructor cleaned up
        }
    }
}
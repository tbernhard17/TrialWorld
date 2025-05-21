using System;

namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Represents a timestamp with associated transcript text in searchable content
    /// </summary>
    public class SearchableTimestamp
    {
        /// <summary>
        /// The text content at this timestamp
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Start time of the timestamp
        /// </summary>
        public TimeSpan Start { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// End time of the timestamp
        /// </summary>
        public TimeSpan End { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Confidence score for this timestamp (0.0-1.0)
        /// </summary>
        public float Confidence { get; set; } = 0.0f;
    }
}
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrialWorld.Infrastructure.Models.AssemblyAI.DTOs
{
    /// <summary>
    /// Represents automatic highlights detected in the transcript.
    /// This is a DTO (Data Transfer Object) that directly maps to the JSON response.
    /// </summary>
    public class AutoHighlightsResultDto
    {
        /// <summary>
        /// Collection of highlight results
        /// </summary>
        [JsonPropertyName("results")]
        public List<HighlightDto>? Results { get; set; }

        /// <summary>
        /// Status of auto highlights detection
        /// </summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    /// <summary>
    /// Represents a single highlight within the auto highlights results.
    /// This is a DTO (Data Transfer Object) that directly maps to the JSON response.
    /// </summary>
    public class HighlightDto
    {
        /// <summary>
        /// The count or importance score of this highlight
        /// </summary>
        [JsonPropertyName("count")]
        public int Count { get; set; }

        /// <summary>
        /// The detected highlight text
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// The rank of this highlight compared to others
        /// </summary>
        [JsonPropertyName("rank")]
        public int Rank { get; set; }

        /// <summary>
        /// Timestamps where this highlight appears
        /// </summary>
        [JsonPropertyName("timestamps")]
        public List<TimestampDto>? Timestamps { get; set; }
    }

    /// <summary>
    /// Represents a timestamp for a highlight occurrence.
    /// This is a DTO (Data Transfer Object) that directly maps to the JSON response.
    /// </summary>
    public class TimestampDto
    {
        /// <summary>
        /// The start time in milliseconds
        /// </summary>
        [JsonPropertyName("start")]
        public int Start { get; set; }

        /// <summary>
        /// The end time in milliseconds
        /// </summary>
        [JsonPropertyName("end")]
        public int End { get; set; }
    }
}

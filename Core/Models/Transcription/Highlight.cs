using System;

namespace TrialWorld.Core.Models.Transcription
{
    /// <summary>
    /// Represents a highlighted segment or key point within a media file transcription.
    /// </summary>
    public class Highlight
    {
        /// <summary>
        /// The text content of the highlight.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// The start time of the highlighted segment.
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// The end time of the highlighted segment.
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// The confidence score or rank associated with this highlight (e.g., AssemblyAI rank 0.0-1.0).
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// The type or reason for the highlight (e.g., "auto_highlight", "keyword_sentence").
        /// </summary>
        public string? Type { get; set; }
    }
}
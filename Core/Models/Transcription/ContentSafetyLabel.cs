using System;

namespace TrialWorld.Core.Models.Transcription
{
    /// <summary>
    /// Represents a content safety label identified within a media file.
    /// </summary>
    public class ContentSafetyLabel
    {
        /// <summary>
        /// The specific safety label identified (e.g., Profanity, Hate Speech).
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// The confidence score for this label detection (0.0 to 1.0).
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// The severity score associated with this label (0.0 to 1.0), if applicable.
        /// </summary>
        public double Severity { get; set; }

        /// <summary>
        /// The start time of the segment where the label applies.
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// The end time of the segment where the label applies.
        /// </summary>
        public TimeSpan EndTime { get; set; }
    }
}
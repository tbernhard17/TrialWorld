using System;

namespace TrialWorld.Core.Models.Transcription
{
    /// <summary>
    /// Represents a named entity detected within the transcription.
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// The text of the detected entity.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// The type of the entity (e.g., PERSON, LOCATION, ORGANIZATION).
        /// </summary>
        public string EntityType { get; set; } = "Unknown";

        /// <summary>
        /// The start time of the entity occurrence.
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// The end time of the entity occurrence.
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// The confidence score for the entity detection (Note: AssemblyAI doesn't typically provide this, often defaulted to 1.0).
        /// </summary>
        public double Confidence { get; set; } = 1.0;
    }
}
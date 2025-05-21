using System.Collections.Generic;

namespace TrialWorld.Core.Models.Transcription
{
    /// <summary>
    /// Represents detailed information about a word in a transcript segment.
    /// </summary>
    public class WordInfo
    {
        /// <summary>
        /// Gets or sets the text of the word.
        /// </summary>
        public string Text { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the start time of the word in milliseconds.
        /// </summary>
        public double StartTime { get; set; }
        
        /// <summary>
        /// Gets or sets the end time of the word in milliseconds.
        /// </summary>
        public double EndTime { get; set; }
        
        /// <summary>
        /// Gets or sets the confidence score for the word recognition (0.0 to 1.0).
        /// </summary>
        public double Confidence { get; set; }
    }

    /// <summary>
    /// Represents a segment of a transcript, typically a sentence or utterance.
    /// </summary>
    public class TranscriptSegment
    {
        /// <summary>
        /// Gets or sets the unique identifier for the segment.
        /// </summary>
        public string? Id { get; set; }
        
        /// <summary>
        /// Gets or sets the identifier for the parent media.
        /// </summary>
        public string? MediaId { get; set; }
        
        /// <summary>
        /// Gets or sets the transcribed text for this segment.
        /// </summary>
        public required string Text { get; set; }
        
        /// <summary>
        /// Gets or sets the start time of the segment in milliseconds.
        /// </summary>
        public double StartTime { get; set; }
        
        /// <summary>
        /// Gets or sets the end time of the segment in milliseconds.
        /// </summary>
        public double EndTime { get; set; }
        
        /// <summary>
        /// Gets or sets the confidence score for this segment (0.0 to 1.0).
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// Gets or sets the speaker identifier for this segment.
        /// </summary>
        public required string Speaker { get; set; }
        
        /// <summary>
        /// Gets or sets the speaker label (e.g., "A", "B", etc.) used for diarization.
        /// </summary>
        public string SpeakerLabel { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the sentiment of this segment (e.g., "POSITIVE", "NEGATIVE", "NEUTRAL").
        /// </summary>
        public required string Sentiment { get; set; }
        
        /// <summary>
        /// Gets or sets the detailed word-level information for this segment.
        /// </summary>
        public List<WordInfo>? Words { get; set; }
    }
}
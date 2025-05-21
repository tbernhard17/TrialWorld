using System.Collections.Generic;

namespace TrialWorld.AssemblyAIDiagnostic.Models
{
    /// <summary>
    /// Represents information about a word in a transcript.
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
        /// Gets or sets the confidence score for the word.
        /// </summary>
        public double Confidence { get; set; }
    }

    /// <summary>
    /// Represents a segment of a transcript.
    /// </summary>
    public class TranscriptSegment
    {
        /// <summary>
        /// Gets or sets the unique identifier for the segment.
        /// </summary>
        public string? Id { get; set; }
        
        /// <summary>
        /// Gets or sets the media identifier associated with this segment.
        /// </summary>
        public string? MediaId { get; set; }
        
        /// <summary>
        /// Gets or sets the text of the segment.
        /// </summary>
        public string Text { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the start time of the segment in milliseconds.
        /// </summary>
        public double StartTime { get; set; }
        
        /// <summary>
        /// Gets or sets the end time of the segment in milliseconds.
        /// </summary>
        public double EndTime { get; set; }
        
        /// <summary>
        /// Gets or sets the start time of the segment in seconds (for AssemblyAI API compatibility).
        /// </summary>
        public double Start { get; set; }
        
        /// <summary>
        /// Gets or sets the end time of the segment in seconds (for AssemblyAI API compatibility).
        /// </summary>
        public double End { get; set; }
        
        /// <summary>
        /// Gets or sets the confidence score for the segment.
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// Gets or sets the speaker label for the segment.
        /// </summary>
        public string? Speaker { get; set; }
        
        /// <summary>
        /// Gets or sets the sentiment for the segment.
        /// </summary>
        public string? Sentiment { get; set; }
        
        /// <summary>
        /// Gets or sets the words in the segment.
        /// </summary>
        public List<WordInfo>? Words { get; set; }
    }
}

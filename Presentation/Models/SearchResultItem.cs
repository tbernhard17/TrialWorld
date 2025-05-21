using System;

namespace TrialWorld.Presentation.Models
{
    /// <summary>
    /// Represents a search result item from a transcript search
    /// </summary>
    public class SearchResultItem
    {
        /// <summary>
        /// Gets or sets the transcript ID
        /// </summary>
        public string TranscriptId { get; set; } = string.Empty;
        // For compatibility with ViewModel code
        public string TranscriptionId {
            get => TranscriptId;
            set => TranscriptId = value;
        }
        
        // Millisecond-based start/end for compatibility
        public int StartTimeMs { get; set; }
        public int EndTimeMs { get; set; }
        
        // Timestamp in milliseconds for seeking in the player
        public int TimestampMs { get => StartTimeMs; set => StartTimeMs = value; }
        
        // Type of search result (Word, Sentiment, Highlight, Chapter)
        public string Type { get; set; } = string.Empty;
        // Sentiment (positive, neutral, negative) if applicable
        public string Sentiment { get; set; } = string.Empty;
        // Formatted time string in mm:ss format
        public string FormattedTime => TimeSpan.FromMilliseconds(StartTimeMs).ToString(@"mm\:ss");
        
        /// <summary>
        /// Gets or sets the text snippet that matched the search
        /// </summary>
        public string Text { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the confidence score (0.0 to 1.0)
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// Gets or sets the language code
        /// </summary>
        public string Language { get; set; } = "en";
        
        /// <summary>
        /// Gets or sets the creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the start time of the match in the media
        /// </summary>
        public TimeSpan StartTime { get; set; }
        
        /// <summary>
        /// Gets or sets the end time of the match in the media
        /// </summary>
        public TimeSpan EndTime { get; set; }
        
        /// <summary>
        /// Gets or sets the speaker identifier
        /// </summary>
        public string Speaker { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the relevance score for this search result
        /// </summary>
        public double RelevanceScore { get; set; }
        
        /// <summary>
        /// Gets a formatted display of the timestamp
        /// </summary>
        public string FormattedTimestamp => $"{StartTime:hh\\:mm\\:ss} - {EndTime:hh\\:mm\\:ss}";
    }
}

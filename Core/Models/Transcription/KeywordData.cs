namespace TrialWorld.Core.Models.Transcription
{
    /// <summary>
    /// Keywords data for media content.
    /// </summary>
    public class KeywordData
    {
        /// <summary>
        /// ID of the media this keyword data belongs to.
        /// </summary>
        public string MediaId { get; set; } = string.Empty;
        
        /// <summary>
        /// The keyword or phrase.
        /// </summary>
        public string Text { get; set; } = string.Empty;
        
        /// <summary>
        /// Timestamp in milliseconds where this keyword appears.
        /// </summary>
        public int TimestampMs { get; set; }
        
        /// <summary>
        /// Duration of the keyword segment in milliseconds.
        /// </summary>
        public int DurationMs { get; set; }
        
        /// <summary>
        /// Confidence score (0.0 to 1.0).
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// Optional category or tag for this keyword.
        /// </summary>
        public string Category { get; set; } = string.Empty;
    }
}

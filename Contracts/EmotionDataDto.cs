using System;

namespace TrialWorld.Contracts
{
    /// <summary>
    /// Represents emotion data detected in media analysis
    /// </summary>
    public class EmotionDataDto
    {
        /// <summary>
        /// Type of emotion (e.g., "happy", "sad", "angry")
        /// </summary>
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// Confidence score of the emotion detection (0.0-1.0)
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// Timestamp when this emotion was detected
        /// </summary>
        public TimeSpan Timestamp { get; set; }
        
        /// <summary>
        /// Duration for which this emotion persisted
        /// </summary>
        public TimeSpan? Duration { get; set; }
        
        /// <summary>
        /// Speaker ID associated with this emotion, if applicable
        /// </summary>
        public string? SpeakerId { get; set; }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public EmotionDataDto()
        {
        }
        
        /// <summary>
        /// Creates a new emotion data entry
        /// </summary>
        /// <param name="type">Type of emotion</param>
        /// <param name="confidence">Confidence score</param>
        /// <param name="timestamp">When emotion was detected</param>
        public EmotionDataDto(string type, double confidence, TimeSpan timestamp)
        {
            Type = type;
            Confidence = confidence;
            Timestamp = timestamp;
        }
    }
}
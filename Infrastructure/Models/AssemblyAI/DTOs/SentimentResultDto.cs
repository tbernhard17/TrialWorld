// [LEGACY DTO REMOVED]
// This file is now obsolete. All sentiment mapping is handled via AssemblyAIMapper and Core models.

using System.Text.Json.Serialization;
using TrialWorld.Core.Models.Transcription;

namespace TrialWorld.Infrastructure.Models.AssemblyAI.DTOs
{
    /// <summary>
    /// INTERNAL: Used only for API transport/serialization. Do not use in domain or business logic.
    /// </summary>
    /// <summary>
    /// Represents a sentiment analysis result from the AssemblyAI API.
    /// </summary>
    public class SentimentResultDto
    {
        /// <summary>
        /// The text that was analyzed for sentiment.
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }
        
        /// <summary>
        /// The start time of the sentiment segment in milliseconds.
        /// </summary>
        [JsonPropertyName("start")]
        public int Start { get; set; }
        
        /// <summary>
        /// The end time of the sentiment segment in milliseconds.
        /// </summary>
        [JsonPropertyName("end")]
        public int End { get; set; }
        
        /// <summary>
        /// The sentiment classification (positive, negative, neutral).
        /// </summary>
        [JsonPropertyName("sentiment")]
        public Sentiment Sentiment { get; set; }
        
        /// <summary>
        /// The confidence score for the sentiment classification (0.0-1.0).
        /// </summary>
        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }
}

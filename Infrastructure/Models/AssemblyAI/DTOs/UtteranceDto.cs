using System.Text.Json.Serialization;

namespace TrialWorld.Infrastructure.Models.AssemblyAI.DTOs
{
    /// <summary>
    /// INTERNAL: Used only for API transport/serialization. Do not use in domain or business logic.
    /// </summary>
    /// <summary>
    /// Represents a speaker utterance in the transcript when speaker diarization is enabled.
    /// This is a DTO (Data Transfer Object) that directly maps to the JSON response.
    /// </summary>
    public class UtteranceDto
    {
        /// <summary>
        /// The start time of the utterance in milliseconds
        /// </summary>
        [JsonPropertyName("start")]
        public int Start { get; set; }

        /// <summary>
        /// The end time of the utterance in milliseconds
        /// </summary>
        [JsonPropertyName("end")]
        public int End { get; set; }

        /// <summary>
        /// The speaker identifier
        /// </summary>
        [JsonPropertyName("speaker")]
        public string? Speaker { get; set; }

        /// <summary>
        /// The transcribed text for this utterance
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// The confidence score for the utterance (0-1)
        /// </summary>
        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }
}

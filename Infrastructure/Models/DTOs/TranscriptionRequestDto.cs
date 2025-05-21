using System.Text.Json.Serialization;

namespace TrialWorld.AssemblyAIDiagnostic.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object for AssemblyAI transcription requests.
    /// </summary>
    public class TranscriptionRequestDto
    {
        /// <summary>
        /// The URL of the audio file to transcribe.
        /// </summary>
        [JsonPropertyName("audio_url")]
        public string AudioUrl { get; set; } = string.Empty;

        /// <summary>
        /// The language code for the audio file.
        /// </summary>
        [JsonPropertyName("language_code")]
        public string? LanguageCode { get; set; }

        /// <summary>
        /// Whether to enable speaker diarization.
        /// </summary>
        [JsonPropertyName("speaker_labels")]
        public bool SpeakerLabels { get; set; }

        /// <summary>
        /// Whether to enable sentiment analysis.
        /// </summary>
        [JsonPropertyName("sentiment_analysis")]
        public bool SentimentAnalysis { get; set; }

        /// <summary>
        /// Whether to enable entity detection.
        /// </summary>
        [JsonPropertyName("entity_detection")]
        public bool EntityDetection { get; set; }
    }
}

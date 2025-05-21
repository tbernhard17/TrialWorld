using System.Text.Json.Serialization;

namespace TrialWorld.Infrastructure.Transcription.DTOs
{
    /// <summary>
    /// Data Transfer Object for AssemblyAI transcription requests.
    /// This is the primary DTO for direct API integration with AssemblyAI.
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

        /// <summary>
        /// Whether to enable auto chapters.
        /// </summary>
        [JsonPropertyName("auto_chapters")]
        public bool AutoChapters { get; set; }

        /// <summary>
        /// Whether to add punctuation to the transcript.
        /// </summary>
        [JsonPropertyName("punctuate")]
        public bool Punctuate { get; set; } = true;

        /// <summary>
        /// Whether to format text with capitalization and punctuation.
        /// </summary>
        [JsonPropertyName("format_text")]
        public bool FormatText { get; set; } = true;

        /// <summary>
        /// The URL to send a webhook to when the transcription is complete.
        /// </summary>
        [JsonPropertyName("webhook_url")]
        public string? WebhookUrl { get; set; }

        /// <summary>
        /// The name of the authorization header to use for the webhook.
        /// </summary>
        [JsonPropertyName("webhook_auth_header_name")]
        public string? WebhookAuthHeaderName { get; set; }

        /// <summary>
        /// The value of the authorization header to use for the webhook.
        /// </summary>
        [JsonPropertyName("webhook_auth_header_value")]
        public string? WebhookAuthHeaderValue { get; set; }

        /// <summary>
        /// The model to use for transcription. Default is "nova".
        /// </summary>
        [JsonPropertyName("model")]
        public string? Model { get; set; } = "nova";
    }
}

using System.Text.Json.Serialization;

namespace TrialWorld.Infrastructure.Models.AssemblyAI.DTOs
{
    /// <summary>
    /// INTERNAL: Used only for API transport/serialization. Do not use in domain or business logic.
    /// </summary>
    /// <summary>
    /// Represents an AssemblyAI transcription request payload.
    /// </summary>
    public class AssemblyAITranscriptionRequestDto
    {
        [JsonPropertyName("audio_url")]
        public string AudioUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("speaker_labels")]
        public bool SpeakerLabels { get; set; } = true;

        [JsonPropertyName("punctuate")]
        public bool Punctuate { get; set; } = true;

        [JsonPropertyName("format_text")]
        public bool FormatText { get; set; } = true;

        [JsonPropertyName("sentiment_analysis")]
        public bool SentimentAnalysis { get; set; } = false;

        [JsonPropertyName("language_code")]
        public string? LanguageCode { get; set; }

        [JsonPropertyName("webhook_url")]
        public string? WebhookUrl { get; set; }

        [JsonPropertyName("webhook_auth_header_name")]
        public string? WebhookAuthHeaderName { get; set; }

        [JsonPropertyName("webhook_auth_header_value")]
        public string? WebhookAuthHeaderValue { get; set; }
    }
}
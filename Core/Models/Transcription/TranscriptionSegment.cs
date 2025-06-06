
using System.Text.Json.Serialization;

namespace TrialWorld.Core.Transcription
{
    public class TranscriptionSegment
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("start")]
        public int Start { get; set; } // Start timestamp in milliseconds

        [JsonPropertyName("end")]
        public int End { get; set; } // End timestamp in milliseconds

        [JsonPropertyName("confidence")]
        public float Confidence { get; set; }

        [JsonPropertyName("speaker")]
        public string Speaker { get; set; } = string.Empty;

        [JsonPropertyName("sentiment")]
        public string Sentiment { get; set; } = string.Empty; // e.g., "POSITIVE", "NEGATIVE", "NEUTRAL"
    }
}

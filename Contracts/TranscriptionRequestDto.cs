using System;

namespace TrialWorld.Contracts
{
    /// <summary>
    /// DTO for transcription requests from clients.
    /// </summary>
    public class TranscriptionRequestDto
    {
        /// <summary>
        /// The ID of the media file to transcribe.
        /// </summary>
        public string MediaId { get; set; } = string.Empty;

        /// <summary>
        /// Optional language code to use for transcription.
        /// </summary>
        public string? LanguageCode { get; set; }

        /// <summary>
        /// Whether to enable speaker diarization (speaker identification).
        /// </summary>
        public bool EnableSpeakerDiarization { get; set; } = false;

        /// <summary>
        /// Whether to enable sentiment analysis.
        /// </summary>
        public bool EnableSentimentAnalysis { get; set; } = true;
    }
}

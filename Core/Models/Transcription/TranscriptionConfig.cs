using System.Collections.Generic;

namespace TrialWorld.Core.Models.Transcription
{
    /// <summary>
    /// Configuration options for the transcription process.
    /// </summary>
    public class TranscriptionConfig
    {
        public string? ModelName { get; set; } = "aai.SpeechModel.universal";
        public string? LanguageCode { get; set; }
        public bool EnableSpeakerDiarization { get; set; } = false;
        public bool EnableEntityDetection { get; set; } = false;
        public bool EnableSummarization { get; set; } = false;
        public bool EnableSentimentAnalysis { get; set; } = true;
        public bool EnableChapters { get; set; } = false;
        public bool EnableAutoHighlights { get; set; } = false;
        public string? WebhookUrl { get; set; }
        public string? WebhookAuthHeaderName { get; set; }
        public string? WebhookAuthHeaderValue { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether silence detection should be performed.
        /// </summary>
        public bool EnableSilenceDetection { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the silence threshold in decibels (dB).
        /// Default is -30dB as per project memory.
        /// </summary>
        public double SilenceThresholdDb { get; set; } = -30.0;
        
        /// <summary>
        /// Gets or sets the minimum silence duration in milliseconds.
        /// Default is 10000ms (10 seconds) as per project memory.
        /// </summary>
        public int MinimumSilenceDurationMs { get; set; } = 10000;
        
        /// <summary>
        /// Gets or sets the list of detected silence segments.
        /// </summary>
        public List<SilenceSegment>? SilenceSegments { get; set; }
    }
}
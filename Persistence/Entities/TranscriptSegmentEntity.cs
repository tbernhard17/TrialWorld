using System;

namespace TrialWorld.Persistence.Entities
{
    /// <summary>
    /// Placeholder entity for TranscriptSegment.
    /// Maps to DB schema, related to Core.Models.Transcription.TranscriptSegment.
    /// </summary>
    public class TranscriptSegmentEntity
    {
        public Guid Id { get; set; } // Or int?
        public string MediaId { get; set; } = string.Empty; // Foreign Key
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Text { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string? SpeakerLabel { get; set; }
        public double? SentimentScore { get; set; } // <-- Added for sentiment analysis

        // Navigation property back to Media?
        // public MediaMetadataEntity Media { get; set; }
    }
} 
using System;

namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Represents a segment of transcribed text in a media file with timing information
    /// </summary>
    public class MediaTranscriptSegment
    {
        /// <summary>
        /// Gets or sets the unique identifier for this segment
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the foreign key to MediaMetadata
        /// </summary>
        public string MediaId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the navigation property to MediaMetadata
        /// </summary>
        public MediaInfo Media { get; set; } = null!;

        /// <summary>
        /// Gets or sets the transcribed text content
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the start time of this segment in seconds
        /// </summary>
        public double StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of this segment in seconds
        /// </summary>
        public double EndTime { get; set; }

        /// <summary>
        /// Gets or sets the confidence score of the transcription (0.0-1.0)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Gets or sets the speaker identifier if speaker diarization is enabled
        /// </summary>
        public string? SpeakerId { get; set; }

        /// <summary>
        /// Gets the speaker (for compatibility with Presentation layer)
        /// </summary>
        public string Speaker => SpeakerId ?? string.Empty;

        /// <summary>
        /// Gets or sets the speaker name if available
        /// </summary>
        public string? SpeakerName { get; set; }

        /// <summary>
        /// Gets or sets whether this segment contains a question
        /// </summary>
        public bool IsQuestion { get; set; }

        /// <summary>
        /// Gets or sets whether this segment is highlighted
        /// </summary>
        public bool IsHighlighted { get; set; }

        /// <summary>
        /// Gets or sets user notes for this segment
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets the path to the frame thumbnail at this segment if available
        /// </summary>
        public string? ThumbnailPath { get; set; }

        /// <summary>
        /// Gets or sets language code of this segment (ISO 639-1)
        /// </summary>
        public string LanguageCode { get; set; } = "en";

        /// <summary>
        /// Gets the duration of this segment in seconds
        /// </summary>
        public double Duration => EndTime - StartTime;

        /// <summary>
        /// Gets or sets the sentence type (statement, question, exclamation)
        /// </summary>
        public string SentenceType { get; set; } = "statement";

        /// <summary>
        /// Gets or sets keywords extracted from this segment
        /// </summary>
        public string[]? Keywords { get; set; }

        /// <summary>
        /// Gets or sets sentiment score for this segment (-1.0 to 1.0)
        /// </summary>
        public double? SentimentScore { get; set; }
    }
}
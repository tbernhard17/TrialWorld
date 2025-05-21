using System;
using System.Collections.Generic;

namespace TrialWorld.Core.Models.Transcription
{
    /// <summary>
    /// Represents a complete transcript of a media file.
    /// </summary>
    public class MediaTranscript
    {
        /// <summary>
        /// Gets or sets the unique identifier for the transcript.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the media identifier associated with this transcript.
        /// </summary>
        public string MediaId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full text of the transcript.
        /// </summary>
        public string FullText { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the segments of the transcript.
        /// </summary>
        public List<TranscriptSegment> Segments { get; set; } = new List<TranscriptSegment>();

        /// <summary>
        /// Gets or sets the language of the transcript.
        /// </summary>
        public string Language { get; set; } = "en";

        /// <summary>
        /// Gets or sets the date the transcript was created.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the original filename of the media.
        /// </summary>
        public string FileName { get; set; } = string.Empty;
    }
}

using System;

namespace TrialWorld.Core.Models.Transcription
{
    /// <summary>
    /// Represents an automatically generated chapter within a media file transcription.
    /// </summary>
    public class Chapter
    {
        /// <summary>
        /// The title or short summary (gist) of the chapter.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// A longer summary or headline for the chapter.
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// The start time of the chapter.
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// The end time of the chapter.
        /// </summary>
        public TimeSpan EndTime { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace TrialWorld.Core.Models.Transcription
{
    /// <summary>
    /// Represents the result of a media pipeline transcription operation
    /// </summary>
    public class MediaPipelineTranscriptionResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the transcription was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error message if the transcription failed
        /// </summary>
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the path to the transcript file
        /// </summary>
        public string TranscriptPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the time the transcription started
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the time the transcription completed
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets or sets the duration of the transcription in seconds
        /// </summary>
        public double DurationInSeconds { get; set; }

        /// <summary>
        /// Gets or sets the segments of the transcript
        /// </summary>
        public TranscriptSegment[] Segments { get; set; } = Array.Empty<TranscriptSegment>();

        /// <summary>
        /// Gets or sets the extracted metadata from the transcript
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Represents a segment of a transcript with timing information
        /// </summary>
        
    }
}
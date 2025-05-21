using System;
using System.Collections.Generic;

namespace TrialWorld.Contracts
{
    /// <summary>
    /// Represents the status of a media processing job.
    /// </summary>
    public class MediaProcessingStatusDto
    {
        /// <summary>
        /// Current overall status (e.g., queued, processing, completed, failed).
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// General status message.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// ID of the media being processed.
        /// </summary>
        public string? MediaId { get; set; }

        /// <summary>
        /// Overall progress percentage (0-100).
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        /// Current operation being performed.
        /// </summary>
        public string? CurrentOperation { get; set; }

        /// <summary>
        /// Time the processing job started.
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Time the processing job ended (if completed or failed).
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Error message if the job failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Status of individual processing steps (e.g., "Transcription": "Completed").
        /// </summary>
        public Dictionary<string, string>? StepStatus { get; set; } = new();
    }
}
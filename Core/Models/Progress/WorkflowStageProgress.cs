using System;

namespace TrialWorld.Core.Models.Progress
{
    /// <summary>
    /// Represents the progress of a specific stage within a larger workflow for a media file.
    /// </summary>
    public class WorkflowStageProgress
    {
        /// <summary>
        /// Identifier for the media file being processed (e.g., file path or unique ID).
        /// </summary>
        public required string FileIdentifier { get; set; }

        /// <summary>
        /// Name of the current processing stage (e.g., "Extracting Audio", "Uploading", "Transcribing").
        /// </summary>
        public required string StageName { get; set; }

        /// <summary>
        /// Progress percentage for the current stage (0-100). 
        /// Null if progress is indeterminate or not applicable.
        /// Renamed from Percentage to match usage.
        /// </summary>
        public double? ProgressPercentage { get; set; }

        /// <summary>
        /// Optional status message providing more details about the current stage or step.
        /// </summary>
        public string? StatusMessage { get; set; }

        /// <summary>
        /// High-level status of the stage (e.g., "Processing", "Completed", "Failed"). Added to match usage.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Detailed message, potentially an error message. Added to match usage.
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Indicates if the current stage's progress calculation is indeterminate (e.g., waiting for external service).
        /// </summary>
        public bool IsIndeterminate { get; set; } = false;

        /// <summary>
        /// Indicates if the current stage has completed successfully.
        /// </summary>
        public bool IsComplete { get; set; } = false;

        /// <summary>
        /// Indicates if an error occurred during this stage.
        /// </summary>
        public bool IsError { get; set; } = false;

        /// <summary>
        /// Error message if <see cref="IsError"/> is true.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Timestamp when this progress update was generated.
        /// </summary>
        public DateTime Timestamp { get; } = DateTime.UtcNow;

        // Constructor could be added if needed for initialization logic
        // public WorkflowStageProgress(string fileIdentifier, string stageName) { ... }
    }
}
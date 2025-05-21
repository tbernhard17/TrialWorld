using TrialWorld.Core.Interfaces;

namespace TrialWorld.Core.Models.Transcription
{
    /// <summary>
    /// Represents an update on transcription progress.
    /// </summary>
    public class TranscriptionProgressUpdate
    {
        /// <summary>
        /// Gets or sets the transcription ID.
        /// </summary>
        public string TranscriptionId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the file path being processed.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the current phase of the transcription process.
        /// </summary>
        public TranscriptionPhase Phase { get; set; }
        
        /// <summary>
        /// Gets or sets the progress percentage (0-100) - legacy property for backward compatibility.
        /// </summary>
        public double ProgressPercent { get; set; }
        
        /// <summary>
        /// Gets or sets the current status of the transcription process - legacy property for backward compatibility.
        /// </summary>
        public TranscriptionStatus Status { get; set; }
        
        /// <summary>
        /// Gets or sets the progress percentage (0-100) of the current phase.
        /// </summary>
        public double PhaseProgress { get; set; }
        
        /// <summary>
        /// Gets or sets the overall progress percentage (0-100) of the transcription process.
        /// </summary>
        public double OverallProgress { get; set; }
        
        /// <summary>
        /// Gets or sets a message describing the current status.
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets a flag indicating whether the transcription is complete.
        /// </summary>
        public bool IsComplete { get; set; }
        
        /// <summary>
        /// Gets or sets a flag indicating whether the transcription has failed.
        /// </summary>
        public bool HasFailed { get; set; }
        
        /// <summary>
        /// Gets or sets an error message if the transcription has failed.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}

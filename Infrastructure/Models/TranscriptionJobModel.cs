using System;
using TrialWorld.Infrastructure.Transcription.Configuration;

namespace TrialWorld.AssemblyAIDiagnostic.Models
{
    /// <summary>
    /// Represents a transcription job in the queue.
    /// </summary>
    public class TranscriptionJobModel
    {
        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the transcription ID.
        /// </summary>
        public string TranscriptionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public TranscriptionStatus Status { get; set; } = TranscriptionStatus.NotStarted;

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the time when the job was submitted.
        /// </summary>
        public DateTime SubmittedAt { get; set; }

        /// <summary>
        /// Gets or sets the time when the job was completed.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Gets the duration of the job.
        /// </summary>
        public TimeSpan Duration => CompletedAt.HasValue ? CompletedAt.Value - SubmittedAt : DateTime.Now - SubmittedAt;

        /// <summary>
        /// Gets the formatted duration of the job.
        /// </summary>
        public string FormattedDuration
        {
            get
            {
                var duration = Duration;
                return duration.TotalHours >= 1
                    ? $"{duration.Hours}h {duration.Minutes}m {duration.Seconds}s"
                    : duration.TotalMinutes >= 1
                        ? $"{duration.Minutes}m {duration.Seconds}s"
                        : $"{duration.Seconds}s";
            }
        }

        /// <summary>
        /// Gets the status text.
        /// </summary>
        public string StatusText => Status.ToString();
    }
}

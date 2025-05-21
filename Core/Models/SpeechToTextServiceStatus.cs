namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Status information about the speech-to-text service.
    /// </summary>
    public class SpeechToTextServiceStatus
    {
        /// <summary>
        /// Gets or sets whether the service is available.
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Gets or sets whether the service is healthy.
        /// </summary>
        public bool IsHealthy { get; set; }

        /// <summary>
        /// Gets or sets the number of queued jobs.
        /// </summary>
        public int QueuedJobs { get; set; }

        /// <summary>
        /// Gets or sets the number of active jobs.
        /// </summary>
        public int ActiveJobs { get; set; }

        /// <summary>
        /// Gets or sets the API error message, if any.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the account information.
        /// </summary>
        public required string AccountInfo { get; set; }

        /// <summary>
        /// Gets or sets the usage percentage.
        /// </summary>
        public double UsagePercentage { get; set; }

        /// <summary>
        /// Gets or sets the remaining credits.
        /// </summary>
        public int RemainingCredits { get; set; }
    }
}
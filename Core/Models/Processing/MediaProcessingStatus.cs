namespace TrialWorld.Core.Models.Processing
{
    /// <summary>
    /// Status of a media processing operation
    /// </summary>
    public enum MediaProcessingStatus
    {
        /// <summary>
        /// Initial state, not yet started
        /// </summary>
        Unknown,

        /// <summary>
        /// Waiting to be processed
        /// </summary>
        Queued,

        /// <summary>
        /// Currently being processed
        /// </summary>
        Processing,

        /// <summary>
        /// Processing is paused
        /// </summary>
        Paused,

        /// <summary>
        /// Successfully completed
        /// </summary>
        Completed,

        /// <summary>
        /// Failed with errors
        /// </summary>
        Failed,

        /// <summary>
        /// Canceled by user or system
        /// </summary>
        Cancelled
    }
}
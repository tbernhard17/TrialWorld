using System;

namespace TrialWorld.Core.Media.Models
{
    /// <summary>
    /// Event arguments for enhancement progress notifications
    /// </summary>
    public class EnhancementProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the progress percentage (0-100)
        /// </summary>
        public double ProgressPercentage { get; }
        
        /// <summary>
        /// Gets the elapsed time since the enhancement started
        /// </summary>
        public TimeSpan ElapsedTime { get; }
        
        /// <summary>
        /// Gets a message describing the current operation
        /// </summary>
        public string StatusMessage { get; }
        
        /// <summary>
        /// Gets the current frame number (if applicable)
        /// </summary>
        public long? CurrentFrame { get; }
        
        /// <summary>
        /// Gets the total number of frames (if applicable)
        /// </summary>
        public long? TotalFrames { get; }
        
        /// <summary>
        /// Gets the estimated remaining time (if available)
        /// </summary>
        public TimeSpan? EstimatedRemainingTime { get; }
        
        /// <summary>
        /// Gets whether this is the finalizing stage of processing
        /// </summary>
        public bool IsFinalizingStage { get; }
        
        /// <summary>
        /// Initializes a new instance of the EnhancementProgressEventArgs class
        /// </summary>
        /// <param name="progress">The progress percentage (0-100)</param>
        /// <param name="elapsed">The elapsed time since the enhancement started</param>
        /// <param name="statusMessage">A message describing the current operation</param>
        /// <param name="currentFrame">The current frame number (optional)</param>
        /// <param name="totalFrames">The total number of frames (optional)</param>
        /// <param name="remainingTime">The estimated remaining time (optional)</param>
        /// <param name="isFinalizingStage">Whether this is the finalizing stage of processing</param>
        public EnhancementProgressEventArgs(
            double progress,
            TimeSpan elapsed,
            string statusMessage,
            long? currentFrame = null,
            long? totalFrames = null,
            TimeSpan? remainingTime = null,
            bool isFinalizingStage = false)
        {
            ProgressPercentage = Math.Clamp(progress, 0, 100);
            ElapsedTime = elapsed;
            StatusMessage = statusMessage ?? string.Empty;
            CurrentFrame = currentFrame;
            TotalFrames = totalFrames;
            EstimatedRemainingTime = remainingTime;
            IsFinalizingStage = isFinalizingStage;
        }
        
        /// <summary>
        /// Initializes a new instance of the EnhancementProgressEventArgs class with just progress and status
        /// </summary>
        /// <param name="progressPercentage">Current progress percentage (0-100)</param>
        /// <param name="status">Current status message</param>
        public EnhancementProgressEventArgs(double progressPercentage, string status)
            : this(progressPercentage, TimeSpan.Zero, status)
        {
        }
    }
}
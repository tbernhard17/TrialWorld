namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Represents the current state of media playback
    /// </summary>
    public enum PlaybackState
    {
        /// <summary>
        /// Media is stopped and position is at the beginning
        /// </summary>
        Stopped,

        /// <summary>
        /// Media is currently playing
        /// </summary>
        Playing,

        /// <summary>
        /// Media is paused at current position
        /// </summary>
        Paused,

        /// <summary>
        /// Media is buffering/loading
        /// </summary>
        Buffering,

        /// <summary>
        /// Media has reached the end
        /// </summary>
        Ended,

        /// <summary>
        /// An error occurred during playback
        /// </summary>
        Error,

        /// <summary>
        /// Media is seeking to a new position
        /// </summary>
        Seeking
    }
}
namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Specifies the mode for seeking operations in media files
    /// </summary>
    public enum SeekMode
    {
        /// <summary>
        /// Accurate seek to the exact frame (slower but precise)
        /// </summary>
        Accurate = 0,

        /// <summary>
        /// Fast seek to the nearest keyframe before the target position
        /// </summary>
        FastBackward = 1,

        /// <summary>
        /// Fast seek to the nearest keyframe after the target position
        /// </summary>
        FastForward = 2,

        /// <summary>
        /// Seek to the nearest keyframe before the target position
        /// </summary>
        KeyframeBefore = 3,

        /// <summary>
        /// Seek to the nearest keyframe after the target position
        /// </summary>
        KeyframeAfter = 4
    }
}
namespace TrialWorld.Core.Models.Transcription
{
    /// <summary>
    /// Represents the different transcription models available.
    /// </summary>
    public enum TranscriptionModel
    {
        /// <summary>
        /// Nova model (previously called "Nano") - fastest, good for most use cases.
        /// </summary>
        Nova = 0,
        
        /// <summary>
        /// Universal model - balanced speed and accuracy.
        /// </summary>
        Universal = 1,
        
        /// <summary>
        /// High accuracy model (SLAM-1) - highest accuracy for complex audio.
        /// </summary>
        HighAccuracy = 2
    }
}

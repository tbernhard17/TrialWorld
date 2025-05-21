namespace TrialWorld.Contracts
{
    /// <summary>
    /// Defines the parameters for initiating media processing.
    /// </summary>
    public class MediaProcessingRequestDto
    {
        /// <summary>
        /// ID of the media to process.
        /// </summary>
        public string? MediaId { get; set; }

        /// <summary>
        /// Optional enhancement profile to apply.
        /// </summary>
        public string? EnhancementProfile { get; set; }

        /// <summary>
        /// Whether to perform transcription.
        /// </summary>
        public bool Transcribe { get; set; } = true; // Default to true?

        /// <summary>
        /// Whether to perform face analysis.
        /// </summary>
        public bool FaceAnalysis { get; set; } = true; // Default to true?

        /// <summary>
        /// Whether to perform emotion analysis.
        /// </summary>
        public bool EmotionAnalysis { get; set; } = true; // Default to true?
    }
}
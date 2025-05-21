using System;

namespace TrialWorld.Core.Media.Models
{
    /// <summary>
    /// Basic options for simple media enhancement operations.
    /// For more advanced options, use the full EnhancementOptions class.
    /// </summary>
    /// <remarks>
    /// This class provides a simplified subset of enhancement options for basic use cases.
    /// </remarks>
    public class BasicEnhancementOptions
    {
        /// <summary>
        /// Gets or sets the brightness adjustment level (-100 to 100).
        /// </summary>
        public double Brightness { get; set; } = 0;
        
        /// <summary>
        /// Gets or sets the contrast adjustment level (-100 to 100).
        /// </summary>
        public double Contrast { get; set; } = 0;
        
        /// <summary>
        /// Gets or sets the saturation adjustment level (-100 to 100).
        /// </summary>
        public double Saturation { get; set; } = 0;
        
        /// <summary>
        /// Gets or sets the sharpness adjustment level (0 to 100).
        /// </summary>
        public double Sharpness { get; set; } = 0;
        
        /// <summary>
        /// Gets or sets whether to apply noise reduction.
        /// </summary>
        public bool ReduceNoise { get; set; } = false;
        
        /// <summary>
        /// Gets or sets whether to apply video stabilization.
        /// </summary>
        public bool Stabilize { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the output format (e.g., "mp4", "mov").
        /// </summary>
        public string OutputFormat { get; set; } = "mp4";
        
        /// <summary>
        /// Gets or sets the output quality (0-100).
        /// </summary>
        public int OutputQuality { get; set; } = 80;
    }

    // Note: The EnhancementProgressEventArgs class is now defined in a separate file
    // and follows proper immutability patterns with read-only properties

    public class EnhancementResult
    {
        public bool Success { get; set; }
        public string OutputPath { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class PreviewResult
    {
        public bool Success { get; set; }
        public byte[] PreviewImageData { get; set; } = Array.Empty<byte>();
        public string ErrorMessage { get; set; } = string.Empty;
    }

    // Note: PreviewResult is used for enhancement previews
}

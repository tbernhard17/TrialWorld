using System;

namespace TrialWorld.Core.Models.Configuration
{
    /// <summary>
    /// Configuration for transcription performance optimization
    /// </summary>
    public class TranscriptionPerformanceConfig
    {
        /// <summary>
        /// Hardware acceleration mode to use for FFmpeg processing
        /// </summary>
        public TrialWorld.Core.Models.HardwareAccelerationMode HardwareAcceleration { get; set; } = TrialWorld.Core.Models.HardwareAccelerationMode.Auto;
        
        /// <summary>
        /// Number of threads to use for CPU processing (0 = auto)
        /// </summary>
        public int Threads { get; set; } = 0;
        
        /// <summary>
        /// Quality preset for encoding (lower = faster but lower quality)
        /// </summary>
        public string QualityPreset { get; set; } = "medium";
        
        /// <summary>
        /// Maximum RAM to use for transcoding buffers (in MB, 0 = no limit)
        /// </summary>
        public int MaxMemoryMB { get; set; } = 0;
    }
}

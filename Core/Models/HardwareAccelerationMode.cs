namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Specifies the hardware acceleration mode for media processing
    /// </summary>
    public enum HardwareAccelerationMode
    {
        /// <summary>
        /// No hardware acceleration
        /// </summary>
        None = 0,

        /// <summary>
        /// Automatically select the best available hardware acceleration
        /// </summary>
        Auto = 1,

        /// <summary>
        /// NVIDIA CUDA GPU acceleration
        /// </summary>
        CUDA = 2,

        /// <summary>
        /// NVIDIA NVENC hardware encoding
        /// </summary>
        NVENC = 3,

        /// <summary>
        /// Intel Quick Sync Video acceleration
        /// </summary>
        QuickSync = 4,

        /// <summary>
        /// AMD Advanced Media Framework acceleration
        /// </summary>
        AMF = 5,

        /// <summary>
        /// Video Acceleration API (Linux)
        /// </summary>
        VAAPI = 6,

        /// <summary>
        /// Video Decode and Presentation API for Unix
        /// </summary>
        VDPAU = 7,

        /// <summary>
        /// Apple VideoToolbox acceleration
        /// </summary>
        VideoToolbox = 8
    }
}
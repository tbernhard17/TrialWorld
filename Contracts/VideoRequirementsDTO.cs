using System;

namespace TrialWorld.Contracts
{
    /// <summary>
    /// Defines requirements for video content validation
    /// </summary>
    public class VideoRequirementsDTO
    {
        /// <summary>
        /// Minimum acceptable width resolution in pixels
        /// </summary>
        public required int MinimumResolutionWidth { get; set; }
        
        /// <summary>
        /// Minimum acceptable height resolution in pixels
        /// </summary>
        public required int MinimumResolutionHeight { get; set; }
        
        /// <summary>
        /// Minimum acceptable framerate in frames per second
        /// </summary>
        public required float MinimumFramerate { get; set; }
        
        /// <summary>
        /// Minimum acceptable bitrate in kilobits per second
        /// </summary>
        public required int MinimumBitrateKbps { get; set; }
        
        /// <summary>
        /// Whether the video must be in color (not black and white)
        /// </summary>
        public required bool RequireColor { get; set; }
        
        /// <summary>
        /// Whether the video must have a stable image (not too shaky)
        /// </summary>
        public required bool RequireStableImage { get; set; }
        
        /// <summary>
        /// Maximum allowed duration for the video in seconds (0 for no limit)
        /// </summary>
        public int MaxDurationSeconds { get; set; } = 0;
        
        /// <summary>
        /// Allowed video file formats (e.g., mp4, mov)
        /// </summary>
        public string[]? AllowedFormats { get; set; }
        
        /// <summary>
        /// Creates a default set of video requirements for standard quality
        /// </summary>
        public static VideoRequirementsDTO CreateDefault()
        {
            return new VideoRequirementsDTO
            {
                MinimumResolutionWidth = 640,
                MinimumResolutionHeight = 480,
                MinimumFramerate = 24,
                MinimumBitrateKbps = 1000,
                RequireColor = true,
                RequireStableImage = true,
                AllowedFormats = new[] { "mp4", "mov", "avi", "mkv" }
            };
        }
        
        /// <summary>
        /// Creates a set of video requirements for high definition quality
        /// </summary>
        public static VideoRequirementsDTO CreateHD()
        {
            return new VideoRequirementsDTO
            {
                MinimumResolutionWidth = 1280,
                MinimumResolutionHeight = 720,
                MinimumFramerate = 30,
                MinimumBitrateKbps = 2500,
                RequireColor = true,
                RequireStableImage = true,
                AllowedFormats = new[] { "mp4", "mov", "mkv" }
            };
        }
    }
}

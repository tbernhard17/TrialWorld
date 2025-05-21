using System.Collections.Generic;

namespace TrialWorld.Contracts
{
    /// <summary>
    /// Defines the parameters for requesting media enhancement.
    /// </summary>
    public class MediaEnhancementRequestDto
    {
        /// <summary>
        /// ID of the media to enhance.
        /// </summary>
        public string? MediaId { get; set; }

        /// <summary>
        /// Example flag (keep or remove based on actual needs).
        /// </summary>
        public bool BoostSpeech { get; set; }

        /// <summary>
        /// Dictionary of audio filters to apply (e.g., "volume": "1.5").
        /// </summary>
        public Dictionary<string, object>? AudioFilters { get; set; } = new();

        /// <summary>
        /// Dictionary of video filters to apply (e.g., "brightness": "0.1").
        /// </summary>
        public Dictionary<string, object>? VideoFilters { get; set; } = new();

        /// <summary>
        /// Desired output file name (including extension).
        /// </summary>
        public string? OutputFileName { get; set; }

        /// <summary>
        /// Desired output format (e.g., "mp4", "mp3").
        /// </summary>
        public string? OutputFormat { get; set; }
    }
}
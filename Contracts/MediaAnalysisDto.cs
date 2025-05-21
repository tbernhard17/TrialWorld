using System;
using System.Collections.Generic;

namespace TrialWorld.Contracts
{
    /// <summary>
    /// Represents analysis results for a media item.
    /// </summary>
    public class MediaAnalysisDto
    {
        /// <summary>
        /// ID of the media analyzed.
        /// </summary>
        public string? MediaId { get; set; }

        /// <summary>
        /// General summary of the analysis.
        /// </summary>
        public string? Summary { get; set; }

        /// <summary>
        /// Emotions detected in the media.
        /// </summary>
        public List<EmotionDataDto>? Emotions { get; set; } = new();

        /// <summary>
        /// Faces detected in the media.
        /// </summary>
        public List<FaceDataDto>? Faces { get; set; } = new();

        /// <summary>
        /// Keywords extracted from the media.
        /// </summary>
        public List<string>? Keywords { get; set; } = new();

        /// <summary>
        /// Topics identified in the media.
        /// </summary>
        public List<string>? Topics { get; set; } = new();
    }
}
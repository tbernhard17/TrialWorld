using System;
using System.Collections.Generic;
using TrialWorld.Core.Interfaces;

namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Core models used across the application.
    /// Note: Processing-specific models have been moved to TrialWorld.Core.Models.Processing namespace.
    /// </summary>
    /// <remarks>
    /// The following models have been moved:
    /// - MediaProcessingResult → TrialWorld.Core.Models.Processing.MediaProcessingResult
    /// - MediaMetadata → TrialWorld.Core.Models.MediaMetadata
    /// - ProcessingOptions → TrialWorld.Core.Models.Processing.ProcessingOptions
    /// - VideoEnhancementOptions → TrialWorld.Core.Models.Processing.VideoEnhancementOptions
    /// </remarks>

    // ValidationResult has been moved to TrialWorld.Core.Models.Validation.ValidationResult

    public class SearchFeedback
    {
        public Guid Id { get; set; }
        public string Query { get; set; } = string.Empty;
        public string Feedback { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? UserId { get; set; }
    }

    // IModelStatistics interface moved to TrialWorld.Core.Interfaces namespace
}

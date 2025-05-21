using System;
using System.Collections.Generic;
using TrialWorld.Core.Models.Processing; // For MediaProcessingStatus

namespace TrialWorld.Persistence.Entities
{
    /// <summary>
    /// Placeholder entity for MediaMetadata.
    /// Maps to DB schema, related to Core.Models.MediaMetadata.
    /// </summary>
    public class MediaMetadataEntity
    {
        public required string Id { get; set; } // Or Guid?
        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public MediaProcessingStatus Status { get; set; } // Map enum correctly

        // Add properties for relationships if needed (e.g., Foreign Keys or Navigation Properties)
        // public List<TranscriptSegmentEntity> TranscriptSegments { get; set; } = new();

        // Add other properties corresponding to DB columns
        public long FileSize { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        // etc.
    }
} 
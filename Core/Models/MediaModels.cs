using System;
using System.Collections.Generic;
using TrialWorld.Core.Enums;
using TrialWorld.Core.Models.Transcription; // Added for consolidated TranscriptSegment

namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Represents metadata for a media file.
    /// </summary>
    public class MediaMetadata
    {
        /// <summary>
        /// Unique identifier for the media.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Title of the media.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Original file name.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Path to the media file on disk.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Content type (MIME type).
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// General type of media (Audio, Video, Image, etc.).
        /// </summary>
        public MediaType MediaType { get; set; } = MediaType.Unknown;

        /// <summary>
        /// Name of the source database file this metadata was loaded from.
        /// </summary>
        public string SourceDatabaseFile { get; set; } = string.Empty;

        /// <summary>
        /// URL to access the thumbnail.
        /// </summary>
        public string ThumbnailUrl { get; set; } = string.Empty;

        /// <summary>
        /// Size of the file in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Duration of the media, if applicable.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Date the media was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date the media was last modified.
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Whether the media has been transcribed.
        /// </summary>
        public bool IsTranscribed { get; set; }

        /// <summary>
        /// Additional metadata as key-value pairs.
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// List of faces detected in the media.
        /// </summary>
        public List<FaceData> Faces { get; set; } = new List<FaceData>();

        // Add relationship to TranscriptSegments
        public List<TranscriptSegment> TranscriptSegments { get; set; } = new List<TranscriptSegment>();
    }

    /// <summary>
    /// Represents emotion data detected in media.
    /// </summary>
    public class EmotionData
    {
        /// <summary>
        /// Type of emotion (e.g., happy, sad, angry).
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Confidence score (0.0-1.0).
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// Timestamp in the media when the emotion was detected.
        /// </summary>
        public TimeSpan Timestamp { get; set; }
    }

    /// <summary>
    /// Represents face data detected in media.
    /// </summary>
    public class FaceData
    {
        /// <summary>
        /// Unique identifier for the face.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Bounding box coordinates [x, y, width, height].
        /// </summary>
        public float[] BoundingBox { get; set; } = Array.Empty<float>();

        /// <summary>
        /// Confidence score (0.0-1.0).
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// Timestamp in the media when the face was detected.
        /// </summary>
        public TimeSpan Timestamp { get; set; }
    }

    /// <summary>
    /// Result of a media save operation.
    /// </summary>
    public class MediaSaveResult
    {
        /// <summary>
        /// ID of the saved media.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if the operation failed.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Data for updating a media item.
    /// </summary>
    public class MediaUpdateData
    {
        /// <summary>
        /// New title for the media.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Updated metadata for the media.
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// Updated transcription status.
        /// </summary>
        public bool? IsTranscribed { get; set; }
    }

    /// <summary>
    /// Represents a full transcript for a media file.
    /// </summary>
}
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrialWorld.Contracts
{
    /// <summary>
    /// DTO for media upload result
    /// </summary>
    public class MediaUploadResultDto
    {
        /// <summary>
        /// Unique identifier for the uploaded media
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Title of the uploaded media
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Content type of the uploaded media
        /// </summary>
        [JsonPropertyName("contentType")]
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Size of the uploaded file in bytes
        /// </summary>
        [JsonPropertyName("fileSize")]
        public long FileSize { get; set; }

        /// <summary>
        /// Whether the media is queued for processing
        /// </summary>
        [JsonPropertyName("isQueued")]
        public bool IsQueued { get; set; }

        /// <summary>
        /// Position in the processing queue
        /// </summary>
        [JsonPropertyName("queuePosition")]
        public int QueuePosition { get; set; }
    }

    /// <summary>
    /// DTO for media update operations
    /// </summary>
    public class MediaUpdateDto
    {
        /// <summary>
        /// New title for the media
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Updated metadata for the media
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, string>? Metadata { get; set; }
    }

    /// <summary>
    /// Data for media update operations
    /// </summary>
    public class MediaUpdateDataDto
    {
        /// <summary>
        /// New title for the media
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Updated metadata for the media
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Transcription;
using TrialWorld.Core.StreamInfo;

namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Provides mapping from raw MediaInfo to internal MediaMetadata format.
    /// Safely handles nulls, fallbacks, and future extensibility.
    /// </summary>
    public static class MediaMetadataMapper
    {
        /// <summary>
        /// Maps raw MediaInfo into the MediaMetadata domain model.
        /// </summary>
        /// <param name="mediaInfo">Raw media probe output.</param>
        /// <param name="options">Optional mapping config.</param>
        /// <param name="logger">Optional logger for diagnostics.</param>
        /// <returns>Populated MediaMetadata instance.</returns>
        public static MediaMetadata Map(
            MediaInfo mediaInfo,
            MediaMappingOptions? options = null,
            Action<Exception>? logger = null)
        {
            if (mediaInfo == null) throw new ArgumentNullException(nameof(mediaInfo));

            try
            {
                var metadata = new MediaMetadata
                {
                    Id = mediaInfo.Id ?? Guid.NewGuid().ToString(),
                    Title = Path.GetFileNameWithoutExtension(mediaInfo.FilePath),
                    FileName = Path.GetFileName(mediaInfo.FilePath),
                    FilePath = mediaInfo.FilePath,
                    ContentType = mediaInfo.HasVideo ? "video" : (mediaInfo.HasAudio ? "audio" : "unknown"),
                    ThumbnailUrl = string.Empty, // Set elsewhere
                    FileSize = File.Exists(mediaInfo.FilePath) ? new FileInfo(mediaInfo.FilePath).Length : 0,
                    CreatedDate = mediaInfo.CreationDate?.ToUniversalTime() ?? DateTime.UtcNow,
                    ModifiedDate = mediaInfo.LastModified.ToUniversalTime(),
                    IsTranscribed = false,
                    Metadata = mediaInfo.Format?.Tags ?? new Dictionary<string, string>(),
                    Faces = new List<FaceData>(), // Set elsewhere
                    TranscriptSegments = mediaInfo.TranscriptSegments ?? new List<TranscriptSegment>()
                };

                if (options?.IncludeTechnicalMetrics == true && mediaInfo.Format != null)
                {
                    metadata.Metadata["bitrate"] = mediaInfo.Format.BitRate ?? "unknown";
                    metadata.Metadata["format"] = mediaInfo.Format.FormatName ?? "unknown";
                }

                return metadata;
            }
            catch (Exception ex)
            {
                logger?.Invoke(ex);
                return new MediaMetadata
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Mapping Error",
                    FilePath = mediaInfo?.FilePath ?? "unknown",
                    Metadata = new Dictionary<string, string> { { "error", ex.Message } },
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };
            }
        }
    }

    /// <summary>
    /// Optional feature flags for mapping customization.
    /// </summary>
    public class MediaMappingOptions
    {
        public bool IncludeTechnicalMetrics { get; set; } = false;
    }
}

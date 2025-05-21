using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Interface for exporting media clips with enhancements applied
    /// </summary>
    public interface IExportService
    {
        /// <summary>
        /// Export a clip with the current enhancement settings
        /// </summary>
        /// <param name="sourcePath">Path to the source media file</param>
        /// <param name="outputPath">Path where the exported file will be saved</param>
        /// <param name="startTime">Start time of the clip to export</param>
        /// <param name="endTime">End time of the clip to export</param>
        /// <param name="videoFilters">Video filters to apply during export</param>
        /// <param name="audioFilters">Audio filters to apply during export</param>
        /// <param name="exportFormat">Format of the exported file</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Path to the exported file if successful, null otherwise</returns>
        Task<string?> ExportClipAsync(
            string sourcePath,
            string outputPath,
            TimeSpan startTime,
            TimeSpan endTime,
            VideoFilterChain? videoFilters = null,
            AudioFilterChain? audioFilters = null,
            ExportFormat exportFormat = ExportFormat.MP4,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the default output directory for exports
        /// </summary>
        /// <returns>The default directory path</returns>
        Task<string> GetDefaultExportDirectoryAsync();

        /// <summary>
        /// Export metadata along with the clip
        /// </summary>
        /// <param name="clipPath">Path to the exported clip</param>
        /// <param name="metadata">Metadata to export</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Path to the metadata file if successful, null otherwise</returns>
        Task<string?> ExportMetadataAsync(
            string clipPath,
            ExportMetadata metadata,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets or sets the quality level for exports (0-100)
        /// </summary>
        int ExportQuality { get; set; }

        /// <summary>
        /// Gets or sets whether to include metadata with exports
        /// </summary>
        bool IncludeMetadata { get; set; }
    }

    /// <summary>
    /// Export format options
    /// </summary>
    public enum ExportFormat
    {
        /// <summary>MP4 format with H.264 codec</summary>
        MP4,

        /// <summary>MKV format</summary>
        MKV,

        /// <summary>WebM format</summary>
        WebM,

        /// <summary>MOV format</summary>
        MOV,

        /// <summary>MP3 audio only</summary>
        MP3,

        /// <summary>AAC audio only</summary>
        AAC,

        /// <summary>WAV audio only</summary>
        WAV
    }

    /// <summary>
    /// Metadata for exported clips
    /// </summary>
    public class ExportMetadata
    {
        /// <summary>
        /// Title of the clip
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Description of the clip
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Tags associated with the clip
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Source file information
        /// </summary>
        public string SourceFile { get; set; } = string.Empty;

        /// <summary>
        /// Start time of the clip in the source file
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// End time of the clip in the source file
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Export date and time
        /// </summary>
        public DateTime ExportDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Video filter chain applied during export
        /// </summary>
        public VideoFilterChain? VideoFilters { get; set; }

        /// <summary>
        /// Audio filter chain applied during export
        /// </summary>
        public AudioFilterChain? AudioFilters { get; set; }

        /// <summary>
        /// Export format used
        /// </summary>
        public ExportFormat Format { get; set; }

        /// <summary>
        /// Custom properties for additional metadata
        /// </summary>
        public Dictionary<string, string> CustomProperties { get; set; } = new();
    }
}
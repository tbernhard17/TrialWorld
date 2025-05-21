using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Progress;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Service responsible for initiating media file processing.
    /// (Note: Actual queueing and status management are handled elsewhere).
    /// </summary>
    public interface IQueuedMediaIngestionService
    {
        /// <summary>
        /// Initiates the processing of a media file.
        /// </summary>
        /// <param name="filePath">Path to the media file.</param>
        /// <param name="options">Processing options.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>ID of the processing job that was initiated.</returns>
        Task<string> EnqueueAsync(string filePath, IngestionOptions options, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Options for media ingestion
    /// </summary>
    public class IngestionOptions
    {
        /// <summary>
        /// Number of thumbnails to generate
        /// </summary>
        public int ThumbnailCount { get; init; } = 3;

        /// <summary>
        /// Whether to perform face detection
        /// </summary>
        public bool DetectFaces { get; init; } = true;

        /// <summary>
        /// Whether to analyze emotions
        /// </summary>
        public bool AnalyzeEmotions { get; init; } = true;

        /// <summary>
        /// Whether to generate transcription
        /// </summary>
        public bool GenerateTranscript { get; init; } = true;

        /// <summary>
        /// Whether to use local ML.NET for fallback processing
        /// </summary>
        public bool UseLocalFallback { get; init; } = true;

        /// <summary>
        /// Maximum number of retry attempts for failed operations
        /// </summary>
        public int MaxRetries { get; init; } = 3;

        /// <summary>
        /// Directory where processed files should be moved
        /// </summary>
        public string ProcessedDirectory { get; init; } = "Processed";

        /// <summary>
        /// Directory where metadata files should be stored
        /// </summary>
        public string MetadataDirectory { get; init; } = "Metadata";
    }

    /// <summary>
    /// Status of a media ingestion task
    /// </summary>
    public class IngestionStatus
    {
        /// <summary>
        /// Current state of the ingestion task
        /// </summary>
        public IngestionState State { get; init; }

        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        public int Progress { get; init; }

        /// <summary>
        /// Error message if the task failed
        /// </summary>
        public string? Error { get; init; }

        /// <summary>
        /// Number of retry attempts made
        /// </summary>
        public int RetryCount { get; init; }

        /// <summary>
        /// Timestamp when the task was started
        /// </summary>
        public DateTime StartTime { get; init; }

        /// <summary>
        /// Timestamp when the task was completed or failed
        /// </summary>
        public DateTime? EndTime { get; init; }

        /// <summary>
        /// Path to the generated metadata file
        /// </summary>
        public string? MetadataPath { get; init; }

        /// <summary>
        /// Paths to generated thumbnail files
        /// </summary>
        public IReadOnlyList<string> ThumbnailPaths { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Path to the processed file
        /// </summary>
        public string? ProcessedFilePath { get; init; }
    }

    /// <summary>
    /// State of a media ingestion task
    /// </summary>
    public enum IngestionState
    {
        /// <summary>
        /// Task is queued but not yet started
        /// </summary>
        Queued,

        /// <summary>
        /// Task is currently processing
        /// </summary>
        Processing,

        /// <summary>
        /// Task completed successfully
        /// </summary>
        Completed,

        /// <summary>
        /// Task failed and may be retried
        /// </summary>
        Failed,

        /// <summary>
        /// Task was cancelled
        /// </summary>
        Cancelled,

        /// <summary>
        /// Task failed and exceeded retry limit
        /// </summary>
        FailedPermanently
    }

    /// <summary>
    /// Event arguments for ingestion status changes
    /// </summary>
    public class IngestionStatusEventArgs : EventArgs
    {
        /// <summary>
        /// ID of the ingestion task
        /// </summary>
        public required string TaskId { get; init; }

        /// <summary>
        /// Current status of the task
        /// </summary>
        public required IngestionStatus Status { get; init; }
    }
}

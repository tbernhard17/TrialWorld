using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TrialWorld.Core.StreamInfo;
using TrialWorld.Core.Models.Processing;
using TrialWorld.Core.Models;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Defines the core functionality for processing media files.
    /// </summary>
    public interface IMediaProcessingService
    {
        /// <summary>
        /// Processes a media file asynchronously.
        /// </summary>
        /// <param name="filePath">The path to the media file to process.</param>
        /// <param name="outputDirectory">Optional directory where processed files should be stored.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A result object containing metadata about the processed file.</returns>
        Task<MediaProcessingResult> ProcessAsync(
            string filePath,
            string? outputDirectory = default,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Extracts metadata from a media file without fully processing it.
        /// </summary>
        /// <param name="filePath">The path to the media file.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Metadata about the media file (using detailed model).</returns>
        Task<MediaInfo> ExtractMetadataAsync(
            string filePath,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Process a media file
        /// </summary>
        /// <param name="inputPath">Path to the input file</param>
        /// <param name="options">Processing options</param>
        /// <param name="progress">Optional progress reporter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Processing result</returns>
        Task<MediaProcessingResult> ProcessMediaAsync(
            string inputPath,
            ProcessingOptions options,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Process a batch of media files
        /// </summary>
        /// <param name="inputPaths">Paths to the input files</param>
        /// <param name="options">Processing options</param>
        /// <param name="progress">Optional progress reporter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of processing results</returns>
        Task<IEnumerable<MediaProcessingResult>> ProcessMediaBatchAsync(
            IEnumerable<string> inputPaths,
            ProcessingOptions options,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Converts a media file from one format to another.
        /// </summary>
        /// <param name="sourceFilePath">Path to the source media file.</param>
        /// <param name="targetFormat">The target format string (e.g., "mp4", "mp3").</param>
        /// <param name="options">Optional processing options to apply during conversion.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The path to the converted media file.</returns>
        Task<string> ConvertMediaFormatAsync(
            string sourceFilePath,
            string targetFormat,
            ProcessingOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Enhances a video file based on the provided options.
        /// </summary>
        /// <param name="filePath">Path to the video file.</param>
        /// <param name="options">Enhancement options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task EnhanceVideoAsync(string filePath, VideoEnhancementOptions options, CancellationToken cancellationToken = default);
    }
}
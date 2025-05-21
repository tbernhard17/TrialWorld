using System;
using System.Threading;
using System.Threading.Tasks;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Service for extracting thumbnails from media files.
    /// </summary>
    public interface IThumbnailExtractor
    {
        /// <summary>
        /// Extracts a thumbnail from a media file at the specified position.
        /// </summary>
        /// <param name="mediaPath">Path to the media file.</param>
        /// <param name="outputPath">Path where the thumbnail should be saved.</param>
        /// <param name="position">Position in the media for the thumbnail (default is 5 seconds).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExtractThumbnailAsync(
            string mediaPath,
            string outputPath,
            TimeSpan position = default,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Extracts multiple thumbnails from a media file at regular intervals.
        /// </summary>
        /// <param name="mediaPath">Path to the media file.</param>
        /// <param name="outputDirectory">Directory where thumbnails should be saved.</param>
        /// <param name="count">Number of thumbnails to extract.</param>
        /// <param name="fileNameTemplate">Template for thumbnail file names (e.g., "thumb_{0:D4}.jpg").</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Paths to the extracted thumbnails.</returns>
        Task<string[]> ExtractMultipleThumbnailsAsync(
            string mediaPath,
            string outputDirectory,
            int count,
            string fileNameTemplate = "thumb_{0:D4}.jpg",
            CancellationToken cancellationToken = default);
    }
}
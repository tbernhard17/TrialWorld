using System.Threading;
using System.Threading.Tasks;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for a service that extracts individual frames from video files.
    /// </summary>
    public interface IFrameExtractorService
    {
        /// <summary>
        /// Extracts a single frame from a video file at a specific timestamp.
        /// </summary>
        /// <param name="videoPath">Path to the input video file.</param>
        /// <param name="outputPath">Path where the extracted frame image (e.g., JPG, PNG) should be saved.</param>
        /// <param name="timestamp">The timestamp (in seconds) of the frame to extract.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the path to the extracted frame file.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if required paths are null or empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if timestamp is negative.</exception>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if the input video file does not exist.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if frame extraction fails for other reasons.</exception>
        /// <exception cref="ExternalProcessException">Thrown if the underlying FFmpeg process fails.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled.</exception>
        Task<string> ExtractFrameAsync(
            string videoPath, 
            string outputPath, 
            double timestamp, 
            CancellationToken cancellationToken = default);
    }
} 
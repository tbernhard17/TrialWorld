using System;
using System.Threading;
using System.Threading.Tasks;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for a service that trims media files.
    /// </summary>
    public interface IMediaTrimmerService
    {
        /// <summary>
        /// Trims a media file asynchronously based on start and optional end times.
        /// </summary>
        /// <param name="inputPath">The path to the input media file.</param>
        /// <param name="outputPath">The path where the trimmed media file should be saved.</param>
        /// <param name="startTime">The start time for the trim (in seconds).</param>
        /// <param name="endTime">The optional end time for the trim (in seconds). If null, trims to the end.</param>
        /// <param name="preserveQuality">If true, attempts to use stream copy for faster trimming without re-encoding (less accurate). If false, re-encodes for precise trimming.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the path to the trimmed media file.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if required paths are null or empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if start/end times are invalid.</exception>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if the input media file does not exist.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if trimming fails for other reasons.</exception>
        /// <exception cref="ExternalProcessException">Thrown if the underlying FFmpeg process fails.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled.</exception>
        Task<string> TrimMediaAsync(
            string inputPath,
            string outputPath,
            double startTime,
            double? endTime = null,
            bool preserveQuality = true,
            CancellationToken cancellationToken = default);
    }
} 
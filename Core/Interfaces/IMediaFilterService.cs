using System.Threading;
using System.Threading.Tasks;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Specifies the type of stream to apply filters to.
    /// </summary>
    public enum FilterStreamType { Audio, Video }

    /// <summary>
    /// Defines the contract for a service that applies filter chains to media files.
    /// </summary>
    public interface IMediaFilterService
    {
        /// <summary>
        /// Applies a specified filter chain to either the audio or video stream of a media file.
        /// </summary>
        /// <param name="inputPath">The path to the input media file.</param>
        /// <param name="outputPath">The path where the filtered media file should be saved.</param>
        /// <param name="filterStreamType">The type of stream (Audio or Video) to apply the filter to.</param>
        /// <param name="filterChain">The FFmpeg filter chain string (e.g., "volume=2.0,equalizer=f=1000:width_type=h:width=200:g=-10").</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the path to the filtered media file.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if required paths or filterChain are null or empty.</exception>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if the input media file does not exist.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the file doesn't contain the specified stream type or if filtering fails.</exception>
        /// <exception cref="ExternalProcessException">Thrown if the underlying FFmpeg process fails.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled.</exception>
        Task<string> ApplyFilterAsync(
            string inputPath,
            string outputPath,
            FilterStreamType filterStreamType,
            string filterChain,
            CancellationToken cancellationToken = default);
    }
} 
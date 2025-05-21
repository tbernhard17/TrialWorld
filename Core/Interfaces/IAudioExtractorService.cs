using System.Threading;
using System.Threading.Tasks;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for a service that extracts audio streams from media files.
    /// </summary>
    public interface IAudioExtractorService
    {
        /// <summary>
        /// Extracts the audio stream from a media file asynchronously.
        /// </summary>
        /// <param name="mediaPath">The path to the input media file.</param>
        /// <param name="outputPath">The path where the extracted audio file should be saved.</param>
        /// <param name="format">The desired audio format (e.g., "mp3", "aac", "wav").</param>
        /// <param name="bitrate">The desired audio bitrate in kilobits per second (e.g., 192).</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the path to the extracted audio file.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if required paths or format are null or empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if bitrate is invalid.</exception>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if the input media file does not exist.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the input file has no audio or if extraction fails.</exception>
        /// <exception cref="ExternalProcessException">Thrown if the underlying FFmpeg process fails.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled.</exception>
        Task<string> ExtractAudioAsync(
            string mediaPath,
            string outputPath,
            string format = "mp3",
            int bitrate = 192,
            CancellationToken cancellationToken = default);
    }
} 
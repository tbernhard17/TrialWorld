using System.Threading;
using System.Threading.Tasks;

namespace TrialWorld.Core.Interfaces.Services
{
    /// <summary>
    /// Interface for extracting audio from media files.
    /// </summary>
    public interface IAudioExtractorService
    {
        /// <summary>
        /// Extracts audio from a media file.
        /// </summary>
        /// <param name="inputPath">Path to the input media file.</param>
        /// <param name="outputPath">Path where the extracted audio will be saved.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if extraction was successful, false otherwise.</returns>
        Task<bool> ExtractAudioAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default);
    }
}

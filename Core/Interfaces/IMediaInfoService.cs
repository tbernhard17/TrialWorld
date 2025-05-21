using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models; // Assuming MediaInfo is here

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for a service that retrieves technical information about media files.
    /// </summary>
    public interface IMediaInfoService
    {
        /// <summary>
        /// Probes a media file asynchronously to retrieve detailed information.
        /// </summary>
        /// <param name="filePath">The path to the media file.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="MediaInfo"/>.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if filePath is null or empty.</exception>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if the media file does not exist.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if probing fails or the output cannot be parsed.</exception>
        Task<MediaInfo> GetMediaInfoAsync(string filePath, CancellationToken cancellationToken = default);
    }
} 
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Reads and deserializes metadata from a file (e.g., JSON).
    /// </summary>
    public interface IMetadataFileReader
    {
        /// <summary>
        /// Reads and deserializes media metadata from a file asynchronously.
        /// </summary>
        /// <param name="metadataFilePath">Path to the metadata file.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Deserialized MediaMetadata object, or null if not found/invalid.</returns>
        Task<MediaMetadata?> ReadMetadataFileAsync(string metadataFilePath, CancellationToken cancellationToken = default);
    }
}

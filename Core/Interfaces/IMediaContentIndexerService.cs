using System;
using System.Threading;
using System.Threading.Tasks;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Interface for indexing media content for search functionality.
    /// </summary>
    public interface IMediaContentIndexerService
    {
        /// <summary>
        /// Indexes media content for a specific media ID.
        /// </summary>
        /// <param name="mediaId">The ID of the media to index.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if indexing was successful, false otherwise.</returns>
        Task<bool> IndexMediaContentAsync(string mediaId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Processes and indexes media content.
        /// </summary>
        /// <param name="mediaId">The ID of the media to process and index.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if processing and indexing was successful, false otherwise.</returns>
        Task<bool> ProcessAndIndexMediaAsync(Guid mediaId, CancellationToken cancellationToken = default);
    }
}
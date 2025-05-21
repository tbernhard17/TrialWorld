using System;
using System.Threading;
using System.Threading.Tasks;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for orchestrating the indexing of media content.
    /// </summary>
    public interface IMediaIndexingService
    {
        /// <summary>
        /// Processes a media file (fetches metadata, analysis, transcript) and indexes it.
        /// </summary>
        /// <param name="mediaId">The unique identifier of the media item.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if indexing was successful, false otherwise.</returns>
        Task<bool> ProcessAndIndexMediaAsync(Guid mediaId, CancellationToken cancellationToken = default);
    }
}
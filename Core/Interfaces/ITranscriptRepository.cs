using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using TrialWorld.Core.Models.Transcription;

namespace TrialWorld.Core.Interfaces // Change this namespace
{
    /// <summary>
    /// Defines the contract for storing and retrieving transcripts.
    /// </summary>
    public interface ITranscriptRepository
    {
        /// <summary>
        /// Retrieves a transcript by its unique identifier.
        /// </summary>
        Task<TrialWorld.Core.Models.Transcription.MediaTranscript?> GetTranscriptAsync(string transcriptId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves a transcript.
        /// </summary>
        Task SaveTranscriptAsync(TrialWorld.Core.Models.Transcription.MediaTranscript transcript, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves transcript segments for a given transcript ID.
        /// </summary>
        Task<IList<TranscriptSegment>> GetSegmentsAsync(string transcriptId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a transcript by its ID.
        /// </summary>
        Task DeleteTranscriptAsync(string transcriptId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves transcript segments associated with a specific media ID.
        /// </summary>
        Task<IEnumerable<TranscriptSegment>> GetByMediaIdAsync(Guid mediaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a full MediaTranscript object associated with a specific media ID.
        /// </summary>
        Task<TrialWorld.Core.Models.Transcription.MediaTranscript?> GetByMediaIdAsync(string mediaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds multiple transcript segments.
        /// </summary>
        Task AddSegmentsAsync(IEnumerable<TranscriptSegment> segments, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes transcript segments associated with a specific media ID.
        /// </summary>
        Task DeleteByMediaIdAsync(string mediaId, CancellationToken cancellationToken = default);
    }
}
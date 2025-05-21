using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models.Transcription;

namespace TrialWorld.Core.Services
{
    /// <summary>
    /// Service for managing transcription data in the database
    /// </summary>
    public interface ITranscriptionDatabaseService
    {
        /// <summary>
        /// Gets a transcription by ID
        /// </summary>
        /// <param name="id">The transcription ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The transcription result or null if not found</returns>
        Task<TranscriptionResult?> GetTranscriptionAsync(string id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all transcriptions
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>All transcription results</returns>
        Task<IEnumerable<TranscriptionResult>> GetAllTranscriptionsAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Saves a transcription
        /// </summary>
        /// <param name="transcription">The transcription to save</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> SaveTranscriptionAsync(TranscriptionResult transcription, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Deletes a transcription
        /// </summary>
        /// <param name="id">The transcription ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteTranscriptionAsync(string id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Searches transcriptions
        /// </summary>
        /// <param name="query">The search query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Matching transcription results</returns>
        Task<IEnumerable<TranscriptionResult>> SearchTranscriptionsAsync(string query, CancellationToken cancellationToken = default);
    }
}

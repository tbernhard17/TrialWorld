using System;
using System.Threading;
using System.Threading.Tasks;

namespace TrialWorld.Core.Transcription.Interfaces
{
    /// <summary>
    /// Interface for audio transcription services
    /// </summary>
    public interface ITranscriptionService
    {
        /// <summary>
        /// Starts transcription of an audio file
        /// </summary>
        /// <param name="filePath">Path to the audio file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Transcription ID for tracking</returns>
        Task<string> TranscribeFileAsync(string filePath, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if a transcription is complete
        /// </summary>
        /// <param name="transcriptionId">Transcription ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if transcription is complete</returns>
        Task<bool> IsTranscriptionCompleteAsync(string transcriptionId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the text of a completed transcription
        /// </summary>
        /// <param name="transcriptionId">Transcription ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Transcribed text</returns>
        Task<string> GetTranscriptionTextAsync(string transcriptionId, CancellationToken cancellationToken = default);
    }
}

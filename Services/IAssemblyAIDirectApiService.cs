using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.AssemblyAIDiagnostic.Models;
using TrialWorld.AssemblyAIDiagnostic.Models.DTOs;

namespace TrialWorld.AssemblyAIDiagnostic.Services
{
    /// <summary>
    /// Interface for direct integration with the AssemblyAI REST API.
    /// </summary>
    public interface IAssemblyAIDirectApiService
    {
        /// <summary>
        /// Uploads an audio file to AssemblyAI.
        /// </summary>
        /// <param name="fileStream">The file stream to upload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The URL of the uploaded file.</returns>
        Task<string> UploadFileAsync(Stream fileStream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Submits a transcription request to AssemblyAI.
        /// </summary>
        /// <param name="request">The transcription request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The ID of the submitted transcription.</returns>
        Task<string> SubmitTranscriptionAsync(TranscriptionRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the status of a transcription.
        /// </summary>
        /// <param name="transcriptionId">The ID of the transcription.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The transcription response.</returns>
        Task<TranscriptionResponseDto> GetTranscriptionStatusAsync(string transcriptionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Polls for the completion of a transcription.
        /// </summary>
        /// <param name="transcriptionId">The ID of the transcription.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The completed transcription response.</returns>
        Task<TranscriptionResponseDto> PollForCompletionAsync(string transcriptionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Transcribes an audio file from start to finish.
        /// </summary>
        /// <param name="filePath">The path to the audio file.</param>
        /// <param name="options">Optional transcription options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A MediaTranscript object containing the transcription results.</returns>
        Task<MediaTranscript> TranscribeFileAsync(
            string filePath, 
            TranscriptionRequestDto? options = null, 
            CancellationToken cancellationToken = default);
    }
}

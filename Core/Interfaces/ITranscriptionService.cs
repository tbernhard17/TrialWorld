using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Transcription;
using TrialWorld.Core.Models.Transcription.Interfaces;

namespace TrialWorld.Core.Interfaces
{

    /// <summary>
    /// Defines the different phases of the transcription process.
    /// </summary>
    public enum TranscriptionPhase
    {
        Queued,          // Initial queue
        SilenceDetection, // FFmpeg silence detection
        AudioExtraction, // Extracting audio from video
        Uploading,       // Uploading to API
        Submitted,       // Job submitted to provider
        Processing,      // Provider is processing
        Downloading,     // Downloading results from provider
        Completed,       // Successfully completed
        Failed,          // Failed with errors
        Cancelled        // Cancelled by user
    }

    /// <summary>
    /// Provides functionality for transcribing audio content from media files.
    /// </summary>
    public interface ITranscriptionService
    {
        /// <summary>
        /// Starts transcription for a given local file path, reporting progress.
        /// Returns the final TranscriptionResult upon completion.
        /// </summary>
        /// <param name="filePath">The path to the media file.</param>
        /// <param name="progress">An IProgress instance to report progress updates.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The completed transcription result.</returns>
        /// <summary>
        /// Starts transcription for a given local file path with configuration options, reporting progress.
        /// Returns the final TranscriptionResult upon completion.
        /// </summary>
        /// <param name="filePath">The path to the media file.</param>
        /// <param name="config">Transcription configuration options.</param>
        /// <param name="progress">An IProgress instance to report progress updates.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The completed transcription result.</returns>
        Task<TranscriptionResult> TranscribeWithProgressAsync(
            string filePath,
            TranscriptionConfig config,
            IProgress<TranscriptionProgressUpdate> progress,
            CancellationToken cancellationToken);  
            
        /// <summary>
        /// Starts transcription for a given local file path, reporting progress.
        /// Returns the final TranscriptionResult upon completion.
        /// </summary>
        /// <param name="filePath">The path to the media file.</param>
        /// <param name="progress">An IProgress instance to report progress updates.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The completed transcription result.</returns>
        Task<TranscriptionResult> TranscribeWithProgressAsync(
            string filePath,
            IProgress<TranscriptionProgressUpdate> progress,
            CancellationToken cancellationToken);

        /// <summary>
        /// Transcribes the audio file at the specified path.
        /// </summary>
        /// <param name="filePath">Path to the audio file.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The transcription result.</returns>
        Task<TranscriptionResult> TranscribeAsync(string filePath, CancellationToken cancellationToken);

        /// <summary>
        /// Transcribes the audio file at the specified path with the given configuration.
        /// </summary>
        /// <param name="filePath">Path to the audio file.</param>
        /// <param name="config">Transcription configuration.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The transcription result.</returns>
        Task<TranscriptionResult> TranscribeAsync(string filePath, TranscriptionConfig config, CancellationToken cancellationToken);
        
        /// <summary>
        /// Gets the status of a transcription job.
        /// </summary>
        /// <param name="transcriptionId">ID of the transcription job.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Status of the transcription job.</returns>
        Task<TranscriptionStatus> GetTranscriptionStatusAsync(string transcriptionId, CancellationToken cancellationToken);
        
        /// <summary>
        /// Cancels a transcription job.
        /// </summary>
        /// <param name="transcriptionId">ID of the transcription job to cancel.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if successfully canceled, otherwise false.</returns>
        Task<bool> CancelTranscriptionAsync(string transcriptionId, CancellationToken cancellationToken);
        
        /// <summary>
        /// Downloads the transcription file for a completed transcription
        /// </summary>
        /// <param name="transcriptionId">ID of the transcription to download</param>
        /// <param name="outputPath">Path where the transcription file should be saved</param>
        /// <param name="progress">Optional progress reporter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if download was successful, otherwise false</returns>
        Task<bool> DownloadTranscriptionFileAsync(
            string transcriptionId, 
            string outputPath, 
            IProgress<TranscriptionProgressUpdate>? progress = null, 
            CancellationToken cancellationToken = default);

        // Optional: Add method to retrieve the full transcript object if needed separately
        // Task<FullTranscript> GetFullTranscriptAsync(string transcriptionId, CancellationToken cancellationToken);
    }
}
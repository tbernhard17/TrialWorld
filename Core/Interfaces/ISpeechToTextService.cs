using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Transcription;
using TrialWorld.Core.Models.Progress;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Provides speech-to-text functionality using cloud services.
    /// </summary>
    public interface ISpeechToTextService
    {
        /// <summary>
        /// Transcribes speech from an audio file to text.
        /// </summary>
        /// <param name="audioFilePath">Path to the audio file.</param>
        /// <param name="language">Optional language code (e.g., "en", "fr"). Null for auto-detection.</param>
        /// <param name="progress">Optional progress reporter.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The complete transcript.</returns>
        Task<TrialWorld.Core.Models.Transcription.TranscriptionResult> TranscribeAudioAsync(
            string audioFilePath,
            string? language = default,
            IProgress<WorkflowStageProgress>? progress = default,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Transcribes speech with timestamps for each word.
        /// </summary>
        /// <param name="audioFilePath">Path to the audio file.</param>
        /// <param name="language">Optional language code (e.g., "en", "fr"). Null for auto-detection.</param>
        /// <param name="progress">Optional progress reporter.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Transcript with timestamps for each word.</returns>
        Task<TrialWorld.Core.Models.Transcription.TranscriptionResult> TranscribeAudioWithTimestampsAsync(
            string audioFilePath,
            string? language = default,
            IProgress<WorkflowStageProgress>? progress = default,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the service status and available quota.
        /// </summary>
        /// <returns>The current service status.</returns>
        Task<SpeechToTextServiceStatus> GetServiceStatusAsync();
    }
}
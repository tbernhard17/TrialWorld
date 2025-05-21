using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Service for detecting periods of silence in audio files
    /// </summary>
    public interface ISilenceDetectionService
    {
        /// <summary>
        /// Detects periods of silence in an audio file
        /// </summary>
        /// <param name="filePath">Path to the audio file</param>
        /// <param name="noiseFloorDb">Noise floor in dB (e.g. -30dB)</param>
        /// <param name="minDurationSeconds">Minimum silence duration in seconds</param>
        /// <param name="progress">Progress reporting callback</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of silence periods</returns>
        Task<List<SilencePeriod>> DetectSilenceAsync(
            string filePath, 
            int noiseFloorDb = -30,
            double minDurationSeconds = 10, 
            IProgress<int>? progress = null, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes silence from a media file and saves the result to a new file.
        /// </summary>
        /// <param name="inputFilePath">Source media file path</param>
        /// <param name="outputFilePath">Destination file path for silence-removed media</param>
        /// <param name="noiseFloorDb">Noise floor in dB (e.g. -30dB)</param>
        /// <param name="minDurationSeconds">Minimum silence duration in seconds</param>
        /// <param name="progress">Progress reporting callback (0-100)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Path to the output file</returns>
        Task<string> RemoveSilenceAsync(
            string inputFilePath,
            string outputFilePath,
            int noiseFloorDb = -30,
            double minDurationSeconds = 10,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default);

    }
}

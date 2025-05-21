using System;
using System.Threading;
using System.Threading.Tasks;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for a service that generates waveform data or images from audio files.
    /// </summary>
    public interface IWaveformService
    {
        /// <summary>
        /// Generates normalized waveform data (amplitude peaks) from an audio file.
        /// </summary>
        /// <param name="audioPath">Path to the input audio file.</param>
        /// <param name="sampleCount">The approximate number of data points (samples) to generate.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains an array of floats representing waveform peaks (typically 0.0 to 1.0).</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if audioPath is null or empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if sampleCount is invalid.</exception>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if the input audio file does not exist.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the file has no audio or if generation fails.</exception>
        /// <exception cref="ExternalProcessException">Thrown if the underlying FFmpeg process fails.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled.</exception>
        Task<float[]> GenerateWaveformDataAsync(
            string audioPath,
            int sampleCount = 1000,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a waveform image from an audio file.
        /// </summary>
        /// <param name="audioPath">Path to the input audio file.</param>
        /// <param name="outputPath">Path where the generated waveform image (e.g., PNG) should be saved.</param>
        /// <param name="width">Width of the output image in pixels.</param>
        /// <param name="height">Height of the output image in pixels.</param>
        /// <param name="foregroundColor">Color of the waveform (e.g., "#0066FF" or "blue").</param>
        /// <param name="backgroundColor">Color of the background (e.g., "#FFFFFF" or "white").</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the path to the generated image file.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if required paths are null or empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if width or height are invalid.</exception>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if the input audio file does not exist.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the file has no audio or if image generation fails.</exception>
        /// <exception cref="ExternalProcessException">Thrown if the underlying FFmpeg process fails.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled.</exception>
        Task<string> GenerateWaveformImageAsync(
            string audioPath,
            string outputPath,
            int width = 800,
            int height = 240,
            string foregroundColor = "#0066FF",
            string backgroundColor = "#FFFFFF",
            CancellationToken cancellationToken = default);
    }
} 
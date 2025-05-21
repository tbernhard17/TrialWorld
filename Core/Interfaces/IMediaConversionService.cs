using System;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models.Analysis; // Corrected namespace

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for a service that converts media files between formats and codecs.
    /// </summary>
    public interface IMediaConversionService
    {
        /// <summary>
        /// Converts a media file asynchronously based on the specified options.
        /// </summary>
        /// <param name="inputPath">The path to the input media file.</param>
        /// <param name="outputPath">The path where the converted media file should be saved.</param>
        /// <param name="options">The options specifying conversion parameters (codecs, bitrate, resolution, etc.).</param>
        /// <param name="progress">Optional callback for reporting conversion progress (0.0 to 1.0).</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the path to the converted media file.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if required paths or options are null.</exception>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if the input media file does not exist.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if conversion fails for other reasons.</exception>
        /// <exception cref="ExternalProcessException">Thrown if the underlying FFmpeg process fails.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled.</exception>
        Task<string> ConvertMediaAsync(
            string inputPath,
            string outputPath,
            MediaConversionOptions options,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default);
    }
} 
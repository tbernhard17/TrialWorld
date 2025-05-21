using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for a service that concatenates multiple media files.
    /// </summary>
    public interface IMediaConcatenatorService
    {
        /// <summary>
        /// Concatenates multiple media files into a single output file.
        /// Assumes input files have compatible codecs and formats for stream copying.
        /// </summary>
        /// <param name="inputPaths">An ordered list of paths to the input media files.</param>
        /// <param name="outputPath">The path where the concatenated media file should be saved.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the path to the concatenated file.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if inputPaths or outputPath is null or empty.</exception>
        /// <exception cref="System.ArgumentException">Thrown if inputPaths contains invalid entries.</exception>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if any input file does not exist.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if input files are incompatible or concatenation fails.</exception>
        /// <exception cref="ExternalProcessException">Thrown if the underlying FFmpeg process fails.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled.</exception>
        Task<string> ConcatenateMediaAsync(
            List<string> inputPaths,
            string outputPath,
            CancellationToken cancellationToken = default);
    }
} 
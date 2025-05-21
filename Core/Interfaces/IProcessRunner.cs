using System;
using System.Threading;
using System.Threading.Tasks;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for running external processes.
    /// </summary>
    public interface IProcessRunner
    {
        /// <summary>
        /// Runs an external process asynchronously and returns its standard output.
        /// </summary>
        /// <param name="fileName">The path to the executable file.</param>
        /// <param name="arguments">The command-line arguments.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the standard output of the process.</returns>
        /// <exception cref="ExternalProcessException">Thrown if the process fails to start or exits with a non-zero code.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled.</exception>
        Task<string> RunProcessAsync(string fileName, string arguments, CancellationToken cancellationToken);

        /// <summary>
        /// Runs an external process asynchronously, providing progress updates via standard error.
        /// </summary>
        /// <param name="fileName">The path to the executable file.</param>
        /// <param name="arguments">The command-line arguments.</param>
        /// <param name="progressCallback">Callback for reporting progress lines (typically from stderr).</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ExternalProcessException">Thrown if the process fails to start or exits with a non-zero code.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled.</exception>
        Task RunProcessWithProgressAsync(string fileName, string arguments, IProgress<string> progressCallback, CancellationToken cancellationToken);
    }
}

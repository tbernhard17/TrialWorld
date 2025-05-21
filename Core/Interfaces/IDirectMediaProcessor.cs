using System;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Progress;
using TrialWorld.Core.Models.Processing;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Service responsible for directly processing a media file in a single call.
    /// </summary>
    public interface IDirectMediaProcessor
    {
        /// <summary>
        /// Processes a media file immediately (extracts audio, transcribes, analyzes).
        /// Reports detailed progress via the IProgress interface.
        /// </summary>
        /// <param name="inputPath">Path to the media file.</param>
        /// <param name="progress">Callback for reporting detailed workflow progress.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result object containing paths and analysis data.</returns>
        Task<MediaProcessingResult> ProcessAsync(
            string inputPath,
            IProgress<WorkflowStageProgress>? progress = null,
            CancellationToken cancellationToken = default);
    }
}
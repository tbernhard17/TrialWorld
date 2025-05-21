using System.Threading.Tasks;
using TrialWorld.Core.Models.Processing; // Assuming this is the correct namespace
using System.Threading; // Added for CancellationToken
using TrialWorld.Core.Media.Interfaces; // Added for MediaProcessingOptions
using System;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Placeholder for the overall media processing pipeline interface.
    /// </summary>
    public interface IMediaProcessingPipeline
    {
        Task<bool> InitializeAsync(string inputPath, MediaProcessingOptions options, CancellationToken cancellationToken = default);
        Task StartAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default);
        Task<MediaProcessingStatus> GetStatusAsync();

        // Define methods based on usage, e.g.:
        Task<TrialWorld.Core.Models.Processing.MediaProcessingResult> ProcessMediaAsync(
            string inputPath, 
            ProcessingOptions options, 
            CancellationToken cancellationToken = default);
    }
} 
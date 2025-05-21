using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;

namespace TrialWorld.Presentation.Services
{
    public class DesignTimeSilenceDetectionService : ISilenceDetectionService
    {
        public Task<List<SilencePeriod>> DetectSilenceAsync(
            string mediaFilePath, 
            int noiseFloorDb = -30, 
            double minSilenceDurationSec = 1.0, 
            IProgress<int>? progress = null, 
            CancellationToken cancellationToken = default)
        {
            // Simulate some delay and report progress
            for (int i = 0; i <= 100; i += 20)
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress?.Report(i);
                Task.Delay(50, cancellationToken).Wait(cancellationToken);
            }
            // Return a predefined list of silence periods for design-time
            var periods = new List<SilencePeriod>
            {
                new SilencePeriod { StartTime = TimeSpan.FromSeconds(5), EndTime = TimeSpan.FromSeconds(8), Duration = TimeSpan.FromSeconds(3) },
                new SilencePeriod { StartTime = TimeSpan.FromSeconds(15), EndTime = TimeSpan.FromSeconds(17.5), Duration = TimeSpan.FromSeconds(2.5) }
            };
            return Task.FromResult(periods);
        }

        public Task<string> RemoveSilenceAsync(
            string inputFilePath, 
            string outputFilePath, 
            int noiseFloorDb = -30, 
            double minSilenceDurationSec = 1.0, 
            IProgress<double>? progress = null, 
            CancellationToken cancellationToken = default)
        {
            // Simulate some delay and report progress
            for (int i = 0; i <= 100; i += 10)
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress?.Report(i);
                Task.Delay(100, cancellationToken).Wait(cancellationToken);
            }
            return Task.FromResult(outputFilePath); // Simulate successful operation
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models;

namespace TrialWorld.Core.Interfaces
{
    public interface IABComparisonService
    {
        /// <summary>
        /// Gets whether the A/B comparison is currently active
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Gets the current comparison settings
        /// </summary>
        ComparisonSettings CurrentSettings { get; }

        /// <summary>
        /// Starts an A/B comparison session
        /// </summary>
        /// <param name="originalUrl">URL of the original media stream</param>
        /// <param name="settings">Initial comparison settings</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        Task StartComparisonAsync(string originalUrl, ComparisonSettings settings, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the enhanced stream settings during comparison
        /// </summary>
        /// <param name="settings">New comparison settings to apply</param>
        Task UpdateSettingsAsync(ComparisonSettings settings);

        /// <summary>
        /// Stops the current A/B comparison session
        /// </summary>
        Task StopComparisonAsync();

        /// <summary>
        /// Gets the current comparison statistics
        /// </summary>
        Task<ComparisonStats> GetStatisticsAsync();

        /// <summary>
        /// Event raised when comparison statistics are updated
        /// </summary>
        event EventHandler<ComparisonStats> StatisticsUpdated;
    }

    public class ComparisonSettings
    {
        public string InputPath { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public List<string> EnhancedFilters { get; set; } = new();
        public List<string> OriginalFilters { get; set; } = new();
        public int SwitchIntervalMs { get; set; } = 1000;
        // Legacy fields for interface compatibility
        public string OutputUrl { get; set; } = string.Empty;
        public object? VideoFilters { get; set; }
        public object? AudioFilters { get; set; }
        public bool EnableAudioComparison { get; set; } = true;
        public bool EnableVideoComparison { get; set; } = true;
        public TimeSpan SwitchInterval { get; set; } = TimeSpan.FromSeconds(5);
        public bool RandomizeSwitching { get; set; } = false;
    }

    public class ComparisonStats
    {
        public double ComparisonDuration { get; set; }
        public int SwitchCount { get; set; }
        public double AverageLatency { get; set; }
        public double AverageLatencyMs { get; set; }
        public double MaxLatency { get; set; }
        public int ProcessedFrames { get; set; }
        public int DroppedFrames { get; set; }
        public Dictionary<string, double> FilterMetrics { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Interfaces;
// using TrialWorld.Core.Models; // Obsolete: Enhancement features removed.

namespace TrialWorld.Infrastructure.FFmpeg.Services
{
    public class ABComparisonService : IABComparisonService, IAsyncDisposable
    {
        private readonly ILogger<ABComparisonService> _logger;
        
        private readonly IFFmpegService _ffmpegService;

        private CancellationTokenSource? _cts;
        private Task? _switchingTask;
        private bool _isDisposed;
        private TrialWorld.Core.Interfaces.ComparisonSettings? _settings;
        private readonly Random _rng = new();

        public event EventHandler<TrialWorld.Core.Interfaces.ComparisonStats>? StatisticsUpdated;
        public bool IsActive { get; private set; }
        public TrialWorld.Core.Interfaces.ComparisonSettings CurrentSettings => _settings!;

        public ABComparisonService(
            
            IFFmpegService ffmpegService,
            ILogger<ABComparisonService> logger)
        {
            
            _ffmpegService = ffmpegService ?? throw new ArgumentNullException(nameof(ffmpegService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartComparisonAsync(string inputPath, TrialWorld.Core.Interfaces.ComparisonSettings settings, CancellationToken cancellationToken)
        {
            if (_switchingTask != null)
                throw new InvalidOperationException("Comparison is already running.");

            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            IsActive = true;

            _switchingTask = Task.Run(() => RunSwitchingLoopAsync(_cts.Token), _cts.Token);
            await Task.CompletedTask;
        }

        public async Task UpdateSettingsAsync(TrialWorld.Core.Interfaces.ComparisonSettings settings)
        {
            if (!IsActive)
                throw new InvalidOperationException("No active A/B comparison session");
            _settings = settings;
            await Task.CompletedTask;
        }

        public async Task<TrialWorld.Core.Interfaces.ComparisonStats> GetStatisticsAsync()
        {
            // Dummy implementation for now
            return await Task.FromResult(new TrialWorld.Core.Interfaces.ComparisonStats());
        }

        

        private async Task RunSwitchingLoopAsync(CancellationToken cancellationToken)
        {
            int switchCount = 0;
            double totalLatency = 0;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Enhancement logic removed. Simulate comparison cycle.
                    await Task.Delay(_settings?.SwitchIntervalMs ?? 0, cancellationToken);
                    switchCount++;
                    StatisticsUpdated?.Invoke(this, new TrialWorld.Core.Interfaces.ComparisonStats
                    {
                        SwitchCount = switchCount,
                        AverageLatencyMs = switchCount > 0 ? totalLatency / switchCount : 0,
                    });
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in comparison loop.");
            }
        }

        public async Task StopComparisonAsync()
        {
            if (_cts == null) return;

            _cts.Cancel();

            if (_switchingTask != null)
                await _switchingTask;

            _switchingTask = null;
            _cts.Dispose();
            _cts = null;
            IsActive = false;
        }

        public ValueTask DisposeAsync()
        {
            if (_isDisposed) return ValueTask.CompletedTask;
            _isDisposed = true;

            return new ValueTask(StopComparisonAsync());
        }
    }
}
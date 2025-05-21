using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Processing;

namespace TrialWorld.Core.Services
{
    /// <summary>
    /// Base class for services that need to track and report processing status and progress.
    /// Extends BaseService with progress reporting functionality.
    /// </summary>
    /// <typeparam name="T">The type of the service implementation</typeparam>
    /// <typeparam name="TOptions">The type of options for the service</typeparam>
    public abstract class BaseProcessingService<T, TOptions> : BaseService<T, TOptions>, IProgressTrackable
        where TOptions : class, new()
    {
        private MediaProcessingStatus _currentStatus = MediaProcessingStatus.Unknown;
        private double _currentProgress;
        private readonly SemaphoreSlim _processingLock = new SemaphoreSlim(1, 1);
        private CancellationTokenSource? _cancellationTokenSource;

        public event EventHandler<ProcessingEventArgs>? StatusChanged;

        public MediaProcessingStatus CurrentStatus
        {
            get => _currentStatus;
            protected set
            {
                if (_currentStatus != value)
                {
                    _currentStatus = value;
                    OnStatusChanged(new ProcessingEventArgs(typeof(T).Name, value, _currentProgress));
                }
            }
        }

        public double CurrentProgress
        {
            get => _currentProgress;
            protected set
            {
                if (Math.Abs(_currentProgress - value) > 0.01)
                {
                    _currentProgress = value;
                    OnStatusChanged(new ProcessingEventArgs(typeof(T).Name, _currentStatus, value));
                }
            }
        }

        protected BaseProcessingService(ILogger<T> logger, IOptions<TOptions> options)
            : base(logger, options)
        {
        }

        /// <summary>
        /// Updates both the status and progress in a single call
        /// </summary>
        protected virtual void UpdateProgress(MediaProcessingStatus status, double progress)
        {
            CurrentStatus = status;
            CurrentProgress = progress;
        }

        /// <summary>
        /// Resets the status and progress to initial values
        /// </summary>
        protected virtual void ResetProgress()
        {
            CurrentStatus = MediaProcessingStatus.Unknown;
            CurrentProgress = 0;
        }

        /// <summary>
        /// Creates a new cancellation token source linked to the provided token
        /// </summary>
        protected virtual CancellationTokenSource CreateLinkedTokenSource(CancellationToken cancellationToken)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Dispose();
            }

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            return _cancellationTokenSource;
        }

        /// <summary>
        /// Cancels any ongoing processing
        /// </summary>
        public virtual Task CancelAsync()
        {
            _cancellationTokenSource?.Cancel();
            CurrentStatus = MediaProcessingStatus.Cancelled;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Acquires the processing lock
        /// </summary>
        protected async Task<IDisposable> AcquireProcessingLockAsync(CancellationToken cancellationToken)
        {
            await _processingLock.WaitAsync(cancellationToken);
            return new SemaphoreReleaser(_processingLock);
        }

        /// <summary>
        /// Raises the StatusChanged event
        /// </summary>
        protected virtual void OnStatusChanged(ProcessingEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Logs the processing result and updates status accordingly
        /// </summary>
        protected void ProcessResult(bool success, Exception? error = null)
        {
            if (success)
            {
                CurrentStatus = MediaProcessingStatus.Completed;
                CurrentProgress = 100;
                Logger.LogInformation("Processing completed successfully");
            }
            else
            {
                CurrentStatus = MediaProcessingStatus.Failed;
                if (error != null)
                {
                    Logger.LogError(error, "Processing failed with error");
                    OnStatusChanged(new ProcessingEventArgs(typeof(T).Name, MediaProcessingStatus.Failed, _currentProgress, error));
                }
                else
                {
                    Logger.LogError("Processing failed");
                }
            }
        }

        /// <summary>
        /// Helper class to automatically release a semaphore when disposed
        /// </summary>
        private class SemaphoreReleaser : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;
            private bool _isDisposed;

            public SemaphoreReleaser(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                if (!_isDisposed)
                {
                    _semaphore.Release();
                    _isDisposed = true;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _processingLock.Dispose();
                _cancellationTokenSource?.Dispose();
            }

            base.Dispose(disposing);
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
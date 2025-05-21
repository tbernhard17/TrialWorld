using System;
using TrialWorld.Core.Models.Processing;

namespace TrialWorld.Core.Services
{
    public class ProcessingEventArgs : EventArgs
    {
        public string ServiceName { get; }
        public MediaProcessingStatus Status { get; }
        public double Progress { get; }
        public Exception? Error { get; }

        public ProcessingEventArgs(
            string serviceName,
            MediaProcessingStatus status,
            double progress,
            Exception? error = null)
        {
            ServiceName = serviceName;
            Status = status;
            Progress = progress;
            Error = error;
        }
    }

    public interface IProgressTrackable : IDisposable
    {
        MediaProcessingStatus CurrentStatus { get; }
        double CurrentProgress { get; }
        event EventHandler<ProcessingEventArgs> StatusChanged;
        Task CancelAsync();
    }
}
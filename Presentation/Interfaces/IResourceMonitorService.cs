using System;

namespace TrialWorld.Presentation.Interfaces
{
    public interface IResourceMonitorService
    {
        event EventHandler<ResourceChangedEventArgs> ResourceChanged;
        void Start();
        void Stop();
    }

    public class ResourceChangedEventArgs : EventArgs
    {
        public double CpuUsagePercent { get; set; }
        public double MemoryUsageMB { get; set; }
    }
}

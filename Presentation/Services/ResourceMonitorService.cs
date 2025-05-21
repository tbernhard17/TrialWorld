using System;
using System.Timers;
using TrialWorld.Presentation.Interfaces;

namespace TrialWorld.Presentation.Services
{
    public class ResourceMonitorService : IResourceMonitorService
    {
        public event EventHandler<ResourceChangedEventArgs>? ResourceChanged;

        private System.Timers.Timer? _timer;

        public void Start()
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (s, e) =>
            {
                // Dummy data; replace with real resource monitoring if needed
                ResourceChanged?.Invoke(this, new ResourceChangedEventArgs
                {
                    CpuUsagePercent = 10.0,
                    MemoryUsageMB = 200.0
                });
            };
            _timer.Start();
        }

        public void Stop()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }
    }
}
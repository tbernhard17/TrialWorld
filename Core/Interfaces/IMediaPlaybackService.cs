using System;
using System.Threading;
using System.Threading.Tasks;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Contract for media playback services (e.g., FFmpeg-based player).
    /// </summary>
    public interface IMediaPlaybackService : IDisposable
    {
        Task PlayAsync(string filePath, CancellationToken cancellationToken = default);
        Task PauseAsync(CancellationToken cancellationToken = default);
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}

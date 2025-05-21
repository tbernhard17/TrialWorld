using System;
using System.Threading;
using System.Threading.Tasks;

namespace TrialWorld.Core.Interfaces.Services
{
    /// <summary>
    /// Defines the contract for a service that manages and executes background tasks.
    /// </summary>
    public interface IBackgroundTaskManager
    {
        /// <summary>
        /// Queues a background task for execution.
        /// </summary>
        /// <param name="id">A unique identifier for the task.</param>
        /// <param name="work">The function representing the work to be performed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task QueueTaskAsync(string id, Func<CancellationToken, Task> work);

        // Note: StartAsync and StopAsync are part of IHostedService,
        // which belongs in Infrastructure/Application, not the core interface.
    }
} 
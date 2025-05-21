using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Interfaces.Services; // Added for IBackgroundTaskManager

namespace TrialWorld.Infrastructure.BackgroundJobs // Changed namespace
{
    // Implements both IHostedService (Infra/App concern) and IBackgroundTaskManager (Core contract)
    public class BackgroundTaskManager : IHostedService, IBackgroundTaskManager
    {
        private readonly ConcurrentDictionary<string, BackgroundTask> _tasks = new();
        private readonly ILogger<BackgroundTaskManager> _logger;
        private readonly CancellationTokenSource _shutdownCts = new();

        public BackgroundTaskManager(ILogger<BackgroundTaskManager> logger)
        {
            _logger = logger;
        }

        // Implementation from IBackgroundTaskManager
        public Task QueueTaskAsync(string id, Func<CancellationToken, Task> work)
        {
            if (_tasks.ContainsKey(id))
            {
                _logger.LogWarning("Task with ID {TaskId} already exists. Ignoring queue request.", id);
                return Task.CompletedTask;
            }

            // Pass the RemoveTask method as the completion callback
            var task = new BackgroundTask(id, work, _logger, RemoveTask);
            if (_tasks.TryAdd(id, task))
            {
                 _logger.LogInformation("Task {TaskId} added to tracking dictionary.", id);
                // Fire and forget is okay here as the BackgroundTask handles its own lifecycle via Task.Run
                _ = task.StartAsync(_shutdownCts.Token); 
            }
            else
            {
                 // This should ideally not happen due to the ContainsKey check, but handle defensively
                 _logger.LogError("Failed to add task {TaskId} to tracking dictionary even after check.", id);
            }
            return Task.CompletedTask;
        }

        // Method to remove the task from the dictionary upon completion
        private void RemoveTask(string taskId)
        {
            if (_tasks.TryRemove(taskId, out _))
            {
                _logger.LogInformation("Task {TaskId} removed from tracking dictionary after completion.", taskId);
            }
            else
            {
                _logger.LogWarning("Attempted to remove task {TaskId} but it was not found in the dictionary.", taskId);
            }
        }

        // Implementation from IHostedService
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BackgroundTaskManager Hosted Service is starting.");
            // Nothing explicit to start here, tasks are started when queued.
            return Task.CompletedTask;
        }

        // Implementation from IHostedService
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BackgroundTaskManager Hosted Service is stopping.");
            _shutdownCts.Cancel();
            var stopTasks = new List<Task>();
            foreach (var task in _tasks.Values)
            {
               stopTasks.Add(task.StopAsync());
            }
            await Task.WhenAll(stopTasks);
            _logger.LogInformation("All background tasks stopped.");
        }
    }

    // Internal helper class for managing individual tasks
    // Keeping this internal to the BackgroundTaskManager implementation detail
    internal class BackgroundTask // Changed to internal
    {
        private readonly string _id;
        private readonly Func<CancellationToken, Task> _work;
        private readonly ILogger _logger;
        private readonly Action<string> _onCompleted; // Callback for completion
        private readonly CancellationTokenSource _cts = new();
        private Task? _executingTask;

        public BackgroundTask(string id, Func<CancellationToken, Task> work, ILogger logger, Action<string> onCompleted) // Added onCompleted parameter
        {
            _id = id;
            _work = work;
            _logger = logger;
            _onCompleted = onCompleted; // Store the callback
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
             // Use Task.Run to ensure the work starts on a thread pool thread immediately
             // and doesn't block the caller of QueueTaskAsync
             _executingTask = Task.Run(async () => 
             {
                 using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);
                 try
                 {
                     _logger.LogInformation($"Background task {_id} starting execution.");
                     await _work(linkedCts.Token);
                     _logger.LogInformation($"Background task {_id} completed successfully.");
                 }
                 catch (OperationCanceledException)
                 {
                     if (cancellationToken.IsCancellationRequested)
                     {
                          _logger.LogWarning($"Background task {_id} cancelled due to host shutdown.");
                     }
                     else if (_cts.IsCancellationRequested)
                     {
                          _logger.LogWarning($"Background task {_id} cancelled via StopAsync.");
                     }
                     else
                     {
                         _logger.LogWarning($"Background task {_id} cancelled unexpectedly.");
                     }
                 }
                 catch (Exception ex)
                 {
                     _logger.LogError(ex, $"Background task {_id} failed unexpectedly.");
                     // Consider re-throwing or handling differently based on policy
                 }
                 finally
                 {
                      // Invoke the callback to notify the manager that this task is done
                      _onCompleted?.Invoke(_id);
                      _logger.LogDebug($"Background task {_id} finished execution cycle and notified manager.");
                 }
             }, cancellationToken); // Pass cancellationToken to Task.Run

            // We don't await _executingTask here because StartAsync's purpose is just to start.
            await Task.CompletedTask; 
        }

        public async Task StopAsync()
        {
            if (_executingTask == null || _executingTask.IsCompleted)
                return;

            try
            {
                 if (!_cts.IsCancellationRequested)
                 {
                    _logger.LogInformation($"Requesting cancellation for background task {_id}.");
                    _cts.Cancel();
                 }
                 else
                 {
                    _logger.LogWarning($"Cancellation already requested for background task {_id}.");
                 }
            }
            finally
            {
                // Wait for the task to complete or timeout
                var completedTask = await Task.WhenAny(_executingTask, Task.Delay(TimeSpan.FromSeconds(5)));
                if (completedTask != _executingTask)
                {
                    _logger.LogWarning($"Background task {_id} did not stop within the timeout period.");
                    // Handle timeout case - force kill? Log? Depends on requirements.
                }
                else
                {
                     _logger.LogInformation($"Background task {_id} stopped gracefully.");
                }
            }
        }
    }
} 
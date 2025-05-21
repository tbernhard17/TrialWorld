using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Core.Utilities
{
    /// <summary>
    /// Extension methods for working with Tasks
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Executes a task with a timeout
        /// </summary>
        /// <typeparam name="T">Task result type</typeparam>
        /// <param name="task">Task to execute</param>
        /// <param name="timeout">Timeout duration</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Task result</returns>
        /// <exception cref="TimeoutException">Thrown when the task times out</exception>
        public static async Task<T> TimeoutAfter<T>(this Task<T> task, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            using var timeoutCancellationTokenSource = new CancellationTokenSource();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellationTokenSource.Token);

            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, linkedCts.Token));
            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel(); // Cancel the delay task
                return await task; // Unwrap the task to get the result or propagate exceptions
            }
            else
            {
                throw new TimeoutException($"The operation timed out after {timeout.TotalSeconds} seconds");
            }
        }

        /// <summary>
        /// Executes a task with a timeout
        /// </summary>
        /// <param name="task">Task to execute</param>
        /// <param name="timeout">Timeout duration</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Task</returns>
        /// <exception cref="TimeoutException">Thrown when the task times out</exception>
        public static async Task TimeoutAfter(this Task task, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            using var timeoutCancellationTokenSource = new CancellationTokenSource();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellationTokenSource.Token);

            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, linkedCts.Token));
            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel(); // Cancel the delay task
                await task; // Propagate exceptions if any
            }
            else
            {
                throw new TimeoutException($"The operation timed out after {timeout.TotalSeconds} seconds");
            }
        }

        /// <summary>
        /// Safely waits for a task to complete and logs any exceptions
        /// </summary>
        /// <param name="task">Task to wait for</param>
        /// <param name="logger">Logger for exceptions</param>
        /// <param name="errorMessage">Custom error message for logging</param>
        /// <returns>True if the task completed successfully, false otherwise</returns>
        public static bool SafeWait(this Task task, ILogger logger, string? errorMessage = null)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            try
            {
                task.Wait();
                return true;
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.Flatten().InnerExceptions)
                {
                    logger.LogError(ex, errorMessage ?? "Error waiting for task to complete");
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, errorMessage ?? "Error waiting for task to complete");
                return false;
            }
        }

        /// <summary>
        /// Executes a list of tasks in batches to control parallelism
        /// </summary>
        /// <typeparam name="T">Task result type</typeparam>
        /// <param name="tasks">Collection of tasks to execute</param>
        /// <param name="batchSize">Maximum number of concurrent tasks</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Collection of task results in order of completion</returns>
        public static async Task<IEnumerable<T>> WhenAllBatched<T>(this IEnumerable<Task<T>> tasks, int batchSize, CancellationToken cancellationToken = default)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            if (batchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be greater than zero");

            var taskList = tasks.ToList();
            var results = new List<T>(taskList.Count);

            for (int i = 0; i < taskList.Count; i += batchSize)
            {
                var batch = taskList.Skip(i).Take(batchSize).ToList();
                var batchResults = await Task.WhenAll(batch);
                
                cancellationToken.ThrowIfCancellationRequested();
                
                results.AddRange(batchResults);
            }

            return results;
        }

        /// <summary>
        /// Executes a list of tasks in batches to control parallelism
        /// </summary>
        /// <param name="tasks">Collection of tasks to execute</param>
        /// <param name="batchSize">Maximum number of concurrent tasks</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public static async Task WhenAllBatched(this IEnumerable<Task> tasks, int batchSize, CancellationToken cancellationToken = default)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            if (batchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be greater than zero");

            var taskList = tasks.ToList();

            for (int i = 0; i < taskList.Count; i += batchSize)
            {
                var batch = taskList.Skip(i).Take(batchSize).ToList();
                await Task.WhenAll(batch);
                
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// Ignores all exceptions thrown by a task
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="logger">Optional logger for exceptions</param>
        /// <returns>Task that completes when the original task completes, regardless of exceptions</returns>
        public static async Task IgnoreExceptions(this Task task, ILogger? logger = null)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            try
            {
                await task;
            }
            catch (Exception ex)
            {
                logger?.LogDebug(ex, "Exception in task was ignored");
            }
        }

        /// <summary>
        /// Ignores all exceptions thrown by a task
        /// </summary>
        /// <typeparam name="T">Task result type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="defaultValue">Default value to return if an exception occurs</param>
        /// <param name="logger">Optional logger for exceptions</param>
        /// <returns>Task result or default value if an exception occurs</returns>
        public static async Task<T> IgnoreExceptions<T>(this Task<T> task, T defaultValue = default!, ILogger? logger = null)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                logger?.LogDebug(ex, "Exception in task was ignored");
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely disposes an object after awaiting a task
        /// </summary>
        /// <typeparam name="T">Task result type</typeparam>
        /// <typeparam name="TDisposable">Disposable resource type</typeparam>
        /// <param name="task">Task to await</param>
        /// <param name="disposable">Disposable resource to dispose after task completion</param>
        /// <returns>Task result</returns>
        public static async Task<T> DisposeAfter<T, TDisposable>(this Task<T> task, TDisposable disposable) where TDisposable : IDisposable
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (disposable == null)
                throw new ArgumentNullException(nameof(disposable));

            try
            {
                return await task;
            }
            finally
            {
                disposable.Dispose();
            }
        }

        /// <summary>
        /// Creates a task that completes after the specified delay
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="result">Result value</param>
        /// <param name="delay">Delay duration</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Task that completes after the delay with the specified result</returns>
        public static async Task<T> WithDelay<T>(T result, TimeSpan delay, CancellationToken cancellationToken = default)
        {
            await Task.Delay(delay, cancellationToken);
            return result;
        }
    }
}

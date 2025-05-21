using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Core.Utilities
{
    /// <summary>
    /// A builder class for creating retry policies with configurable options
    /// </summary>
    public class RetryPolicyBuilder
    {
        private int _maxRetries = 3;
        private TimeSpan _delay = TimeSpan.FromMilliseconds(500);
        private TimeSpan? _maxDelay;
        private bool _exponentialBackoff;
        private Func<Exception, bool>? _retryPredicate;
        private ILogger? _logger;
        private Action<Exception, int, TimeSpan>? _onRetry;

        /// <summary>
        /// Sets the maximum number of retry attempts
        /// </summary>
        /// <param name="maxRetries">Maximum number of retries</param>
        /// <returns>This builder instance</returns>
        public RetryPolicyBuilder WithMaxRetries(int maxRetries)
        {
            if (maxRetries < 0)
                throw new ArgumentOutOfRangeException(nameof(maxRetries), "Max retries must be non-negative");
            
            _maxRetries = maxRetries;
            return this;
        }

        /// <summary>
        /// Sets the fixed delay between retry attempts
        /// </summary>
        /// <param name="delay">Delay between retries</param>
        /// <returns>This builder instance</returns>
        public RetryPolicyBuilder WithDelay(TimeSpan delay)
        {
            if (delay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(delay), "Delay must be non-negative");
            
            _delay = delay;
            return this;
        }

        /// <summary>
        /// Sets the maximum delay for exponential backoff
        /// </summary>
        /// <param name="maxDelay">Maximum delay</param>
        /// <returns>This builder instance</returns>
        public RetryPolicyBuilder WithMaxDelay(TimeSpan maxDelay)
        {
            if (maxDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(maxDelay), "Max delay must be non-negative");
            
            _maxDelay = maxDelay;
            return this;
        }

        /// <summary>
        /// Enables exponential backoff for retry delays
        /// </summary>
        /// <returns>This builder instance</returns>
        public RetryPolicyBuilder WithExponentialBackoff()
        {
            _exponentialBackoff = true;
            return this;
        }

        /// <summary>
        /// Sets a predicate to determine if a specific exception should trigger a retry
        /// </summary>
        /// <param name="retryPredicate">Predicate to evaluate exceptions</param>
        /// <returns>This builder instance</returns>
        public RetryPolicyBuilder RetryOnlyIf(Func<Exception, bool> retryPredicate)
        {
            _retryPredicate = retryPredicate ?? throw new ArgumentNullException(nameof(retryPredicate));
            return this;
        }

        /// <summary>
        /// Sets a logger for logging retry operations
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <returns>This builder instance</returns>
        public RetryPolicyBuilder WithLogger(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            return this;
        }

        /// <summary>
        /// Sets an action to execute when a retry occurs
        /// </summary>
        /// <param name="onRetry">Action to execute on retry</param>
        /// <returns>This builder instance</returns>
        public RetryPolicyBuilder OnRetry(Action<Exception, int, TimeSpan> onRetry)
        {
            _onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
            return this;
        }

        /// <summary>
        /// Executes an action with the configured retry policy
        /// </summary>
        /// <param name="action">Action to execute</param>
        public void Execute(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            ExecuteAsync(() => 
            {
                action();
                return Task.CompletedTask;
            }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Executes a function with the configured retry policy
        /// </summary>
        /// <typeparam name="T">Return type of the function</typeparam>
        /// <param name="func">Function to execute</param>
        /// <returns>Result of the function</returns>
        public T Execute<T>(Func<T> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
                
            return ExecuteAsync(() => Task.FromResult(func())).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Executes an async action with the configured retry policy
        /// </summary>
        /// <param name="func">Async action to execute</param>
        /// <returns>Task representing the async operation</returns>
        public async Task ExecuteAsync(Func<Task> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            int retryCount = 0;

            while (true)
            {
                try
                {
                    await func();
                    return;
                }
                catch (Exception ex)
                {
                    if (ShouldRetry(ex, retryCount))
                    {
                        TimeSpan delay = CalculateDelay(retryCount);
                        
                        LogRetry(ex, retryCount, delay);
                        NotifyRetry(ex, retryCount, delay);
                        
                        await Task.Delay(delay);
                        retryCount++;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Executes an async function with the configured retry policy
        /// </summary>
        /// <typeparam name="T">Return type of the function</typeparam>
        /// <param name="func">Async function to execute</param>
        /// <returns>Task representing the async operation with result</returns>
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            int retryCount = 0;

            while (true)
            {
                try
                {
                    return await func();
                }
                catch (Exception ex)
                {
                    if (ShouldRetry(ex, retryCount))
                    {
                        TimeSpan delay = CalculateDelay(retryCount);
                        
                        LogRetry(ex, retryCount, delay);
                        NotifyRetry(ex, retryCount, delay);
                        
                        await Task.Delay(delay);
                        retryCount++;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private bool ShouldRetry(Exception ex, int retryCount)
        {
            if (retryCount >= _maxRetries)
                return false;
                
            if (_retryPredicate != null)
                return _retryPredicate(ex);
                
            return true;
        }

        private TimeSpan CalculateDelay(int retryCount)
        {
            if (!_exponentialBackoff)
                return _delay;
                
            // Calculate exponential delay: delay * 2^retryCount
            TimeSpan expDelay = TimeSpan.FromTicks(_delay.Ticks * (1L << retryCount));
            
            // Cap at maxDelay if specified
            if (_maxDelay.HasValue && expDelay > _maxDelay.Value)
                return _maxDelay.Value;
                
            return expDelay;
        }

        private void LogRetry(Exception ex, int retryCount, TimeSpan delay)
        {
            if (_logger == null) 
                return;
                
            _logger.LogWarning(ex, "Retry {RetryCount} after {Delay}ms, Exception: {ExceptionMessage}", 
                retryCount + 1, delay.TotalMilliseconds, ex.Message);
        }

        private void NotifyRetry(Exception ex, int retryCount, TimeSpan delay)
        {
            _onRetry?.Invoke(ex, retryCount + 1, delay);
        }
    }
}

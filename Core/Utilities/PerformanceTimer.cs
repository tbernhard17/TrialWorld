using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Core.Utilities
{
    /// <summary>
    /// Utility class for measuring and logging execution times of operations
    /// </summary>
    public class PerformanceTimer : IDisposable
    {
        private readonly string _operationName;
        private readonly Stopwatch _stopwatch;
        private readonly ILogger? _logger;
        private readonly LogLevel _logLevel;
        private readonly bool _logOnDispose;
        private readonly Action<TimeSpan>? _onCompleted;
        private static readonly Dictionary<string, TimeSpan> _timings = new Dictionary<string, TimeSpan>();
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the PerformanceTimer class
        /// </summary>
        /// <param name="operationName">Name of the operation being timed</param>
        /// <param name="logger">Optional logger</param>
        /// <param name="logLevel">Log level to use</param>
        /// <param name="logOnDispose">Whether to log when the timer is disposed</param>
        /// <param name="onCompleted">Action to execute when timing completes</param>
        public PerformanceTimer(
            string operationName,
            ILogger? logger = null,
            LogLevel logLevel = LogLevel.Debug,
            bool logOnDispose = true,
            Action<TimeSpan>? onCompleted = null)
        {
            _operationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
            _logger = logger;
            _logLevel = logLevel;
            _logOnDispose = logOnDispose;
            _onCompleted = onCompleted;
            _stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Gets the elapsed time since the timer was started
        /// </summary>
        public TimeSpan Elapsed => _stopwatch.Elapsed;

        /// <summary>
        /// Stops the timer and logs the elapsed time
        /// </summary>
        /// <returns>Elapsed time</returns>
        public TimeSpan Stop()
        {
            _stopwatch.Stop();
            
            TimeSpan elapsed = _stopwatch.Elapsed;
            LogTiming(elapsed);
            
            lock (_lockObject)
            {
                if (_timings.ContainsKey(_operationName))
                {
                    _timings[_operationName] = _timings[_operationName].Add(elapsed);
                }
                else
                {
                    _timings[_operationName] = elapsed;
                }
            }
            
            _onCompleted?.Invoke(elapsed);
            
            return elapsed;
        }

        /// <summary>
        /// Resets and restarts the timer
        /// </summary>
        public void Restart()
        {
            _stopwatch.Restart();
        }

        /// <summary>
        /// Stops the timer and disposes of resources
        /// </summary>
        public void Dispose()
        {
            if (_stopwatch.IsRunning)
            {
                if (_logOnDispose)
                {
                    Stop();
                }
                else
                {
                    _stopwatch.Stop();
                }
            }
        }

        /// <summary>
        /// Logs the elapsed time
        /// </summary>
        /// <param name="elapsed">Elapsed time</param>
        private void LogTiming(TimeSpan elapsed)
        {
            if (_logger == null)
                return;

            string message = $"Operation '{_operationName}' took {FormatTimeSpan(elapsed)}";
            
            switch (_logLevel)
            {
                case LogLevel.Trace:
                    _logger.LogTrace(message);
                    break;
                case LogLevel.Debug:
                    _logger.LogDebug(message);
                    break;
                case LogLevel.Information:
                    _logger.LogInformation(message);
                    break;
                case LogLevel.Warning:
                    _logger.LogWarning(message);
                    break;
                case LogLevel.Error:
                    _logger.LogError(message);
                    break;
                case LogLevel.Critical:
                    _logger.LogCritical(message);
                    break;
                default:
                    _logger.LogDebug(message);
                    break;
            }
        }

        /// <summary>
        /// Formats a TimeSpan for logging
        /// </summary>
        /// <param name="timeSpan">TimeSpan to format</param>
        /// <returns>Formatted string</returns>
        private static string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalMilliseconds < 1)
                return $"{timeSpan.TotalMicroseconds:0.00} μs";
            
            if (timeSpan.TotalSeconds < 1)
                return $"{timeSpan.TotalMilliseconds:0.00} ms";
            
            if (timeSpan.TotalMinutes < 1)
                return $"{timeSpan.TotalSeconds:0.00} sec";
            
            if (timeSpan.TotalHours < 1)
                return $"{timeSpan.TotalMinutes:0.00} min";
            
            return $"{timeSpan.TotalHours:0.00} hr";
        }

        /// <summary>
        /// Gets aggregate performance statistics for all operations
        /// </summary>
        /// <returns>Dictionary of operation names and their total elapsed times</returns>
        public static Dictionary<string, TimeSpan> GetAllTimings()
        {
            lock (_lockObject)
            {
                return new Dictionary<string, TimeSpan>(_timings);
            }
        }

        /// <summary>
        /// Resets all timing statistics
        /// </summary>
        public static void ResetAllTimings()
        {
            lock (_lockObject)
            {
                _timings.Clear();
            }
        }

        /// <summary>
        /// Times the execution of an action
        /// </summary>
        /// <param name="action">Action to time</param>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="logger">Optional logger</param>
        /// <param name="logLevel">Log level to use</param>
        /// <returns>Elapsed time</returns>
        public static TimeSpan Time(Action action, string operationName, ILogger? logger = null, LogLevel logLevel = LogLevel.Debug)
        {
            using var timer = new PerformanceTimer(operationName, logger, logLevel);
            action();
            return timer.Stop();
        }

        /// <summary>
        /// Times the execution of a function
        /// </summary>
        /// <typeparam name="T">Return type of the function</typeparam>
        /// <param name="func">Function to time</param>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="logger">Optional logger</param>
        /// <param name="logLevel">Log level to use</param>
        /// <returns>Tuple containing the function result and elapsed time</returns>
        public static (T Result, TimeSpan Elapsed) Time<T>(Func<T> func, string operationName, ILogger? logger = null, LogLevel logLevel = LogLevel.Debug)
        {
            using var timer = new PerformanceTimer(operationName, logger, logLevel);
            T result = func();
            TimeSpan elapsed = timer.Stop();
            return (result, elapsed);
        }

        /// <summary>
        /// Asynchronously times the execution of a function
        /// </summary>
        /// <typeparam name="T">Return type of the function</typeparam>
        /// <param name="func">Async function to time</param>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="logger">Optional logger</param>
        /// <param name="logLevel">Log level to use</param>
        /// <returns>Tuple containing the function result and elapsed time</returns>
        public static async Task<(T Result, TimeSpan Elapsed)> TimeAsync<T>(Func<Task<T>> func, string operationName, ILogger? logger = null, LogLevel logLevel = LogLevel.Debug)
        {
            using var timer = new PerformanceTimer(operationName, logger, logLevel, false);
            T result = await func();
            TimeSpan elapsed = timer.Stop();
            return (result, elapsed);
        }

        /// <summary>
        /// Asynchronously times the execution of an action
        /// </summary>
        /// <param name="func">Async action to time</param>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="logger">Optional logger</param>
        /// <param name="logLevel">Log level to use</param>
        /// <returns>Elapsed time</returns>
        public static async Task<TimeSpan> TimeAsync(Func<Task> func, string operationName, ILogger? logger = null, LogLevel logLevel = LogLevel.Debug)
        {
            using var timer = new PerformanceTimer(operationName, logger, logLevel, false);
            await func();
            return timer.Stop();
        }
    }
}

using System;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Common.Interfaces;

namespace TrialWorld.Core.Common.Services
{
    public class LoggingService : TrialWorld.Core.Common.Interfaces.ILoggingService // Implements the correct interface
    {
        private readonly ILogger<LoggingService> _logger;

        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Log(LogLevel level, string message, Exception? ex = null) // Added nullable ? to ex
        {
            _logger.Log(level, ex, message);
        }

        public void Log(LogLevel level, string message, object? data = null) // Added nullable ? to data
        {
            // Basic logging, does not directly support structured data like Serilog
            if (data != null)
            {
                _logger.Log(level, "{Message} Data: {Data}", message, data);
            }
            else
            {
                _logger.Log(level, message);
            }
        }
        public void LogTrace(string message, params object?[] args)
        {
            _logger.LogTrace(message, args);
        }

        public void LogDebug(string message, params object?[] args)
        {
            _logger.LogDebug(message, args);
        }

        public void LogInfo(string message, params object?[] args)
        {
            _logger.LogInformation(message, args);
        }

        public void LogWarning(string message, params object?[] args)
        {
            _logger.LogWarning(message, args);
        }

        public void LogError(string message, params object?[] args)
        {
            _logger.LogError(message, args);
        }

        public void LogError(Exception exception, string message, params object?[] args)
        {
            _logger.LogError(exception, message, args);
        }

        public void LogCritical(string message, params object?[] args)
        {
            _logger.LogCritical(message, args);
        }

        public void LogCritical(Exception exception, string message, params object?[] args)
        {
            _logger.LogCritical(exception, message, args);
        }
    }
}

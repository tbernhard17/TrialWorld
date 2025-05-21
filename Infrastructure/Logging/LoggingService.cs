﻿using System;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Common.Interfaces;

namespace TrialWorld.Infrastructure.Logging
{
    /// <summary>
    /// Implementation of the ILoggingService that wraps the Microsoft.Extensions.Logging
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;

        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        public void LogDebug(string message, params object?[] args)
        {
            _logger.LogDebug(message, args);
        }

        public void LogCritical(string message, params object?[] args)
        {
            _logger.LogCritical(message, args);
        }

        public void LogCritical(Exception exception, string message, params object?[] args)
        {
            _logger.LogCritical(exception, message, args);
        }

        public void LogTrace(string message, params object?[] args)
        {
            _logger.LogTrace(message, args);
        }
    }
}

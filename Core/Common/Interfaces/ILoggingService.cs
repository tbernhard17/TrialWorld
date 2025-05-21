using System;

namespace TrialWorld.Core.Common.Interfaces
{
    /// <summary>
    /// Interface for logging services providing methods to log messages at different levels
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// Logs a trace message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional format arguments</param>
        void LogTrace(string message, params object?[] args);

        /// <summary>
        /// Logs a debug message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional format arguments</param>
        void LogDebug(string message, params object?[] args);

        /// <summary>
        /// Logs an information message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional format arguments</param>
        void LogInfo(string message, params object?[] args);

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional format arguments</param>
        void LogWarning(string message, params object?[] args);

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional format arguments</param>
        void LogError(string message, params object?[] args);

        /// <summary>
        /// Logs an error message with an exception
        /// </summary>
        /// <param name="exception">Exception to log</param>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional format arguments</param>
        void LogError(Exception exception, string message, params object?[] args);

        /// <summary>
        /// Logs a critical error message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional format arguments</param>
        void LogCritical(string message, params object?[] args);

        /// <summary>
        /// Logs a critical error message with an exception
        /// </summary>
        /// <param name="exception">Exception to log</param>
        /// <param name="message">Message to log</param>
        /// <param name="args">Optional format arguments</param>
        void LogCritical(Exception exception, string message, params object?[] args);
    }
}

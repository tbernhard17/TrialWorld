using System;
using System.Collections.Generic;

namespace TrialWorld.Contracts.Logging
{
    /// <summary>
    /// Defines logging severity levels
    /// </summary>
    public enum LogSeverity
    {
        /// <summary>
        /// Debug information, most verbose level
        /// </summary>
        Debug,
        
        /// <summary>
        /// General information about system operation
        /// </summary>
        Information,
        
        /// <summary>
        /// Warning conditions that don't affect functionality
        /// </summary>
        Warning,
        
        /// <summary>
        /// Error conditions that affect functionality
        /// </summary>
        Error,
        
        /// <summary>
        /// Critical errors that require immediate attention
        /// </summary>
        Critical
    }
    
    /// <summary>
    /// Contract for structured log entries
    /// </summary>
    public interface ILogEntry
    {
        /// <summary>
        /// The log message
        /// </summary>
        string Message { get; }
        
        /// <summary>
        /// The severity level of the log
        /// </summary>
        LogSeverity Severity { get; }
        
        /// <summary>
        /// The timestamp when the log was created
        /// </summary>
        DateTime Timestamp { get; }
        
        /// <summary>
        /// Additional structured properties for the log entry
        /// </summary>
        Dictionary<string, object> Properties { get; }
    }
    
    /// <summary>
    /// Default implementation of ILogEntry
    /// </summary>
    public class LogEntry : ILogEntry
    {
        /// <summary>
        /// The log message
        /// </summary>
        public string Message { get; }
        
        /// <summary>
        /// The severity level of the log
        /// </summary>
        public LogSeverity Severity { get; }
        
        /// <summary>
        /// The timestamp when the log was created
        /// </summary>
        public DateTime Timestamp { get; }
        
        /// <summary>
        /// Additional structured properties for the log entry
        /// </summary>
        public Dictionary<string, object> Properties { get; }
        
        /// <summary>
        /// Creates a new log entry
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="severity">The severity level</param>
        /// <param name="properties">Additional properties</param>
        public LogEntry(string message, LogSeverity severity, Dictionary<string, object>? properties = null)
        {
            Message = message;
            Severity = severity;
            Timestamp = DateTime.UtcNow;
            Properties = properties ?? new Dictionary<string, object>();
        }
    }
}

using System;

namespace TrialWorld.Core.Exceptions
{
    /// <summary>
    /// Represents errors that occur when running external processes.
    /// </summary>
    public class ExternalProcessException : Exception
    {
        /// <summary>
        /// Gets the exit code returned by the external process, if available.
        /// </summary>
        public int? ExitCode { get; }

        /// <summary>
        /// Gets the error message output (typically stderr) from the external process, if available.
        /// </summary>
        public string? ErrorMessage { get; }

        public ExternalProcessException(string message)
            : base(message) { }

        public ExternalProcessException(string message, Exception innerException)
            : base(message, innerException) { }

        public ExternalProcessException(string message, int? exitCode, string? errorMessage)
            : base(message)
        {
            ExitCode = exitCode;
            ErrorMessage = errorMessage;
        }

        public ExternalProcessException(string message, int? exitCode, string? errorMessage, Exception innerException)
            : base(message, innerException)
        {
            ExitCode = exitCode;
            ErrorMessage = errorMessage;
        }
    }
} 
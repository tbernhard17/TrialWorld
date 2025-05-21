using System;

namespace TrialWorld.Core.Exceptions
{
    /// <summary>
    /// Base exception class for transcription-related errors.
    /// </summary>
    public class TranscriptionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the TranscriptionException class.
        /// </summary>
        public TranscriptionException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the TranscriptionException class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public TranscriptionException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the TranscriptionException class with a specified error message 
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public TranscriptionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when there is an error communicating with the transcription API.
    /// </summary>
    public class TranscriptionApiException : TranscriptionException
    {
        /// <summary>
        /// Gets the HTTP status code of the API error, if applicable.
        /// </summary>
        public int? StatusCode { get; }

        /// <summary>
        /// Initializes a new instance of the TranscriptionApiException class.
        /// </summary>
        public TranscriptionApiException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the TranscriptionApiException class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public TranscriptionApiException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the TranscriptionApiException class with a specified error message
        /// and HTTP status code.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="statusCode">The HTTP status code of the API error.</param>
        public TranscriptionApiException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the TranscriptionApiException class with a specified error message 
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public TranscriptionApiException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the TranscriptionApiException class with a specified error message,
        /// HTTP status code, and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="statusCode">The HTTP status code of the API error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public TranscriptionApiException(string message, int statusCode, Exception innerException) 
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Exception thrown when a transcription operation times out.
    /// </summary>
    public class TranscriptionTimeoutException : TranscriptionException
    {
        /// <summary>
        /// Initializes a new instance of the TranscriptionTimeoutException class.
        /// </summary>
        public TranscriptionTimeoutException() : base("The transcription operation timed out.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the TranscriptionTimeoutException class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public TranscriptionTimeoutException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the TranscriptionTimeoutException class with a specified error message 
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public TranscriptionTimeoutException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

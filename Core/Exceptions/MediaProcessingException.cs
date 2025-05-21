using System;

namespace TrialWorld.Core.Exceptions
{
    /// <summary>
    /// Represents errors that occur during media processing operations (e.g., FFmpeg commands).
    /// </summary>
    public class MediaProcessingException : Exception
    {
        /// <summary>
        /// Gets the path to the media file that was being processed when the error occurred, if available.
        /// </summary>
        public string? FilePath { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaProcessingException"/> class.
        /// </summary>
        public MediaProcessingException()
            : base("An error occurred during media processing.") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaProcessingException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MediaProcessingException(string message)
            : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaProcessingException"/> class with a specified error message 
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public MediaProcessingException(string message, Exception innerException)
            : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaProcessingException"/> class with a specified error message 
        /// and the path to the relevant file.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="filePath">The path to the media file being processed.</param>
        public MediaProcessingException(string message, string? filePath)
            : base(message)
        {
            FilePath = filePath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaProcessingException"/> class with a specified error message, 
        /// the path to the relevant file, and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        /// <param name="filePath">The path to the media file being processed.</param>
        public MediaProcessingException(string message, Exception innerException, string? filePath)
            : base(message, innerException)
        {
            FilePath = filePath;
        }

        // Add any custom properties or constructors if needed later,
        // for example, to hold specific error codes or context.
    }
}
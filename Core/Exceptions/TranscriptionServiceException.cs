using System;

namespace TrialWorld.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a transcription service operation fails
    /// </summary>
    public class TranscriptionServiceException : Exception
    {
        /// <summary>
        /// Initialize a new instance of the TranscriptionServiceException class
        /// </summary>
        public TranscriptionServiceException() : base() { }

        /// <summary>
        /// Initialize a new instance of the TranscriptionServiceException class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public TranscriptionServiceException(string message) : base(message) { }

        /// <summary>
        /// Initialize a new instance of the TranscriptionServiceException class with a specified error
        /// message and a reference to the inner exception that is the cause of this exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public TranscriptionServiceException(string message, Exception innerException) 
            : base(message, innerException) { }
            
        /// <summary>
        /// The transcription ID if available, or null if not applicable
        /// </summary>
        public string? TranscriptionId { get; set; }
        
        /// <summary>
        /// The phase of transcription where the error occurred
        /// </summary>
        public string? Phase { get; set; }
    }
}

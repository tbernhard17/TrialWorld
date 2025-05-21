using System;
using System.Net;

namespace TrialWorld.Core.Exceptions
{
    /// <summary>
    /// Base exception for AssemblyAI API related errors.
    /// </summary>
    public class AssemblyAIApiException : Exception
    {
        /// <summary>
        /// The correlation ID associated with this request for tracing purposes.
        /// </summary>
        public string CorrelationId { get; }

        /// <summary>
        /// The HTTP status code if applicable.
        /// </summary>
        public HttpStatusCode? StatusCode { get; }

        public AssemblyAIApiException(string message, string correlationId, Exception? innerException = null)
            : base(message, innerException)
        {
            CorrelationId = correlationId;
        }

        public AssemblyAIApiException(string message, string correlationId, HttpStatusCode statusCode, Exception? innerException = null)
            : base(message, innerException)
        {
            CorrelationId = correlationId;
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Exception thrown when there's a network-related issue with the AssemblyAI API.
    /// </summary>
    public class AssemblyAINetworkException : AssemblyAIApiException
    {
        public AssemblyAINetworkException(string message, string correlationId, Exception? innerException = null)
            : base($"Network error communicating with AssemblyAI API: {message}", correlationId, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when authentication with the AssemblyAI API fails.
    /// </summary>
    public class AssemblyAIAuthenticationException : AssemblyAIApiException
    {
        public AssemblyAIAuthenticationException(string message, string correlationId, HttpStatusCode statusCode, Exception? innerException = null)
            : base($"Authentication error with AssemblyAI API: {message}", correlationId, statusCode, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when the file format is not supported by AssemblyAI.
    /// </summary>
    public class AssemblyAIFileFormatException : AssemblyAIApiException
    {
        public string FilePath { get; }
        public string FileExtension { get; }

        public AssemblyAIFileFormatException(string message, string correlationId, string filePath, string fileExtension, Exception? innerException = null)
            : base($"File format error: {message}", correlationId, innerException)
        {
            FilePath = filePath;
            FileExtension = fileExtension;
        }
    }

    /// <summary>
    /// Exception thrown when the file size exceeds AssemblyAI's limits.
    /// </summary>
    public class AssemblyAIFileSizeException : AssemblyAIApiException
    {
        public string FilePath { get; }
        public long FileSize { get; }
        public long MaxAllowedSize { get; }

        public AssemblyAIFileSizeException(string message, string correlationId, string filePath, long fileSize, long maxAllowedSize, Exception? innerException = null)
            : base($"File size error: {message}", correlationId, innerException)
        {
            FilePath = filePath;
            FileSize = fileSize;
            MaxAllowedSize = maxAllowedSize;
        }
    }

    /// <summary>
    /// Exception thrown when the AssemblyAI API returns a rate limit error.
    /// </summary>
    public class AssemblyAIRateLimitException : AssemblyAIApiException
    {
        public AssemblyAIRateLimitException(string message, string correlationId, HttpStatusCode statusCode, Exception? innerException = null)
            : base($"Rate limit exceeded: {message}", correlationId, statusCode, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when the AssemblyAI API service experiences internal errors.
    /// </summary>
    public class AssemblyAIServerException : AssemblyAIApiException
    {
        public AssemblyAIServerException(string message, string correlationId, HttpStatusCode statusCode, Exception? innerException = null)
            : base($"AssemblyAI server error: {message}", correlationId, statusCode, innerException)
        {
        }
    }
}
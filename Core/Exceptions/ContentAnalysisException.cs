using System;

namespace TrialWorld.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when content analysis operations fail.
    /// </summary>
    public class ContentAnalysisException : Exception
    {
        public ContentAnalysisException() { }

        public ContentAnalysisException(string message) : base(message) { }

        public ContentAnalysisException(string message, Exception innerException) : base(message, innerException) { }
    }
} 
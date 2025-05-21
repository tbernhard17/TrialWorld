using System;

namespace TrialWorld.Core.Exceptions
{
    public class FFmpegProcessingException : Exception
    {
        public FFmpegProcessingException(string message) : base(message) { }
        public FFmpegProcessingException(string message, Exception inner) : base(message, inner) { }
    }
}

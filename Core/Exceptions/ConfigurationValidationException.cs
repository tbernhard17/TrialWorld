using System;

namespace TrialWorld.Core.Exceptions
{
    public class ConfigurationValidationException : Exception
    {
        public ConfigurationValidationException(string message) : base(message) { }
        public ConfigurationValidationException(string message, Exception inner) : base(message, inner) { }
    }
}
using System;
using TrialWorld.Core.Exceptions;

namespace TrialWorld.Core.Services
{
    public abstract class BaseOptionsValidator<T> where T : class
    {
        protected virtual void ValidateRequired(string value, string propertyName)
        {
            if (string.IsNullOrEmpty(value))
                throw new ConfigurationValidationException($"{propertyName} is required");
        }

        protected virtual void ValidateRequired<TValue>(TValue value, string propertyName)
        {
            if (value == null)
                throw new ConfigurationValidationException($"{propertyName} is required");
        }

        protected virtual void ValidateRange(double value, double min, double max, string propertyName)
        {
            if (value < min || value > max)
                throw new ConfigurationValidationException($"{propertyName} must be between {min} and {max}");
        }

        protected virtual void ValidateUri(string uri, string propertyName)
        {
            if (!Uri.TryCreate(uri, UriKind.Absolute, out _))
                throw new ConfigurationValidationException($"{propertyName} must be a valid URI");
        }

        protected virtual void ValidatePath(string path, string propertyName)
        {
            if (!System.IO.Path.IsPathRooted(path))
                throw new ConfigurationValidationException($"{propertyName} must be an absolute path");
        }

        public abstract void Validate(T options);
    }
}
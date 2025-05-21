using System.Collections.Generic;
using System;

namespace TrialWorld.Core.Models.Validation
{
    public class ValidationResult
    {
        public required string Code { get; set; }
        public required string Message { get; set; }
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; } = new();
        public Dictionary<string, string> Metadata { get; set; } = new();

        public ValidationResult(bool isValid = true)
        {
            IsValid = isValid;
        }

        public void AddError(string code, string message)
        {
            IsValid = false;
            Errors.Add(new ValidationError { Code = code, Message = message });
        }
    }

    public class ValidationError
    {
        public required string Code { get; set; }
        public required string Message { get; set; }
    }
}
namespace TrialWorld.Core.Models.Common
{
    public class OperationResult
    {
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public List<Error> Errors { get; } = new List<Error>();
        public bool Success => !Errors.Any();
        public bool Failed => !Success;
        public Dictionary<string, string> Metadata { get; } = new Dictionary<string, string>();
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        public static OperationResult SuccessResult() => new OperationResult();

        public static OperationResult Failure(string errorCode, string errorMessage) => new OperationResult
        {
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };

        public OperationResult AddError(string errorCode, string errorMessage)
        {
            Errors.Add(new Error(errorCode, errorMessage));
            return this;
        }

        public OperationResult AddMetadata(string key, string value)
        {
            Metadata[key] = value;
            return this;
        }

        public static OperationResult FromException(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }
            return Failure("Exception", exception.Message);
        }

        public class Error
        {
            public string Code { get; }
            public string Message { get; }
            public DateTime Timestamp { get; }

            public Error(string code, string message)
            {
                Code = code ?? throw new ArgumentNullException(nameof(code));
                Message = message ?? throw new ArgumentNullException(nameof(message));
                Timestamp = DateTime.UtcNow;
            }

            public override string ToString()
            {
                return $"[{Code}] {Message}";
            }
        }
    }

    public class OperationResult<T> : OperationResult
    {
        public T? Data { get; set; }

        public static OperationResult<T> SuccessResult(T data) => new OperationResult<T>
        {
            Data = data
        };

        public static new OperationResult<T> Failure(string errorCode, string errorMessage) => new OperationResult<T>
        {
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };
    }

    public static class OperationResultExtensions
    {
        public static OperationResult<T> WithValue<T>(this OperationResult result, T value)
        {
            var operationResult = result.Success ? OperationResult<T>.SuccessResult(value) : new OperationResult<T>
            {
                ErrorCode = result.ErrorCode,
                ErrorMessage = result.ErrorMessage,
                CompletedAt = result.CompletedAt
            };

            foreach (var error in result.Errors)
            {
                operationResult.AddError(error.Code, error.Message);
            }

            foreach (var kvp in result.Metadata)
            {
                operationResult.AddMetadata(kvp.Key, kvp.Value);
            }

            return operationResult;
        }

        public static T GetValueOrThrow<T>(this OperationResult<T> result)
        {
            if (result.Failed || result.Data == null)
            {
                throw new InvalidOperationException("Operation failed or data is null");
            }
            return result.Data;
        }
    }
}
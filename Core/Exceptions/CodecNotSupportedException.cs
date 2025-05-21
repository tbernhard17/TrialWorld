namespace TrialWorld.Core.Exceptions;

public class CodecNotSupportedException : Exception
{
    public CodecNotSupportedException(string message) : base(message)
    {
    }

    public CodecNotSupportedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
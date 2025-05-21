namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Represents the status of a media processing operation.
    /// </summary>
    public class ProcessingStatus
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
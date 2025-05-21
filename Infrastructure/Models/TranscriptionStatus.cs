namespace TrialWorld.AssemblyAIDiagnostic.Models
{
    /// <summary>
    /// Represents the status of a transcription job.
    /// </summary>
    public enum TranscriptionStatus
    {
        NotStarted,
        Queued,
        Processing,
        Completed,
        Error,
        Unknown
    }
}

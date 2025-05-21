using TrialWorld.Core.Models.Transcription;

namespace TrialWorld.Core.Utilities
{
    /// <summary>
    /// Centralized utility for calculating transcription progress based on status and percent complete.
    /// </summary>
    public static class TranscriptionProgressCalculator
    {
        /// <summary>
        /// Calculates progress (0-100) for a transcription job given status and percent complete.
        /// </summary>
        public static double CalculateProgress(string? status, double? percentComplete)
        {
            if (percentComplete.HasValue && percentComplete.Value >= 0 && percentComplete.Value <= 100)
                return percentComplete.Value;

            return status?.ToLowerInvariant() switch
            {
                "queued" => 0,
                "processing" => 10,
                "transcribing" => 50,
                "completed" => 100,
                "failed" => 0,
                _ => 0
            };
        }

        /// <summary>
        /// Calculates progress for a Core TranscriptionResult.
        /// </summary>
        public static double CalculateProgress(TranscriptionResult result)
            => CalculateProgress(result.Status.ToString(), result.PercentComplete);
    }
}

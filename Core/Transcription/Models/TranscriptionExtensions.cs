using System;
using TrialWorld.Core.Models.Transcription;

namespace TrialWorld.Core.Transcription.Models
{
    /// <summary>
    /// Extension methods for working with TranscriptionResult models
    /// </summary>
    public static class TranscriptionExtensions
    {
        /// <summary>
        /// Checks if a transcript is in the completed state
        /// </summary>
        public static bool IsCompleted(this TranscriptionResult response)
        {
            return response != null && response.Status == TranscriptionStatus.Completed;
        }
        
        /// <summary>
        /// Checks if a transcript is in the error state
        /// </summary>
        public static bool HasError(this TranscriptionResult response)
        {
            return response != null && response.Status == TranscriptionStatus.Failed;
        }
        
        /// <summary>
        /// Checks if a transcript is in the processing state
        /// </summary>
        public static bool IsProcessing(this TranscriptionResult response)
        {
            return response != null && response.Status == TranscriptionStatus.Processing;
        }
        
        /// <summary>
        /// Gets a formatted version of the transcript text
        /// </summary>
        public static string GetFormattedText(this TranscriptionResult response)
        {
            if (response == null || string.IsNullOrEmpty(response.Transcript))
            {
                return string.Empty;
            }
            
            return response.Transcript.Trim();
        }
        
        /// <summary>
        /// Gets the current completion percentage
        /// </summary>
        public static int GetProgressPercentage(this TranscriptionResult response)
        {
            if (response == null)
                return 0;
                
            // If completed, return 100%
            if (response.IsCompleted())
                return 100;
                
            // TranscriptionResult doesn't have a ProgressPercent property
            // so we'll just use status-based percentage estimates
                
            // Default to estimated progress based on status
            return response.Status switch
            {
                TranscriptionStatus.Queued => 10,
                TranscriptionStatus.Processing => 50,
                TranscriptionStatus.Completed => 100,
                TranscriptionStatus.Failed => 0,
                TranscriptionStatus.Cancelled => 0,
                _ => 0
            };
        }
        
        /// <summary>
        /// Converts transcript response into displayable text format
        /// </summary>
        /// <param name="transcript">Transcript result</param>
        /// <returns>Formatted text of the transcript</returns>
        public static string ToFormattedText(this TranscriptionResult transcript)
        {
            if (transcript == null || string.IsNullOrEmpty(transcript.Transcript))
            {
                return string.Empty;
            }
            
            return transcript.Transcript;
        }
        
        // Infrastructure layer methods for AssemblyAI API responses have been moved to a dedicated extension class in the Infrastructure layer
        // following the "Layer Ownership is Sacred" principle from our coding standards
    }
}

using System;

namespace TrialWorld.Core.Models.Transcription.Interfaces
{
    /// <summary>
    /// Defines a segment of transcribed audio with text content and timing information.
    /// </summary>
    public interface ITranscriptionSegment
    {
        /// <summary>
        /// Gets the text content of this segment.
        /// </summary>
        string Text { get; }
        
        /// <summary>
        /// Gets the start time of this segment in milliseconds.
        /// </summary>
        int StartMilliseconds { get; }
        
        /// <summary>
        /// Gets the end time of this segment in milliseconds.
        /// </summary>
        int EndMilliseconds { get; }
        
        /// <summary>
        /// Gets the start time as a TimeSpan.
        /// </summary>
        TimeSpan StartTime { get; }
        
        /// <summary>
        /// Gets the end time as a TimeSpan.
        /// </summary>
        TimeSpan EndTime { get; }
        
        /// <summary>
        /// Gets the confidence score for this segment, from 0 to 1.
        /// </summary>
        double Confidence { get; }
    }
}

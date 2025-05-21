using System;

namespace TrialWorld.Core.Models.Transcription
{
    /// <summary>
    /// Represents a segment of silence detected in an audio file.
    /// </summary>
    public class SilenceSegment
    {
        /// <summary>
        /// Gets or sets the start time of the silence segment in milliseconds.
        /// </summary>
        public double StartTimeMs { get; set; }
        
        /// <summary>
        /// Gets or sets the end time of the silence segment in milliseconds.
        /// </summary>
        public double EndTimeMs { get; set; }
        
        /// <summary>
        /// Gets or sets the duration of the silence segment in milliseconds.
        /// </summary>
        public double DurationMs { get; set; }
        
        /// <summary>
        /// Gets or sets the average noise level during this silence segment in decibels.
        /// </summary>
        public double AverageNoiseLevel { get; set; }
        
        /// <summary>
        /// Gets the start time as a TimeSpan.
        /// </summary>
        public TimeSpan StartTime => TimeSpan.FromMilliseconds(StartTimeMs);
        
        /// <summary>
        /// Gets the end time as a TimeSpan.
        /// </summary>
        public TimeSpan EndTime => TimeSpan.FromMilliseconds(EndTimeMs);
        
        /// <summary>
        /// Gets the duration as a TimeSpan.
        /// </summary>
        public TimeSpan Duration => TimeSpan.FromMilliseconds(DurationMs);
        
        /// <summary>
        /// Returns a string representation of the silence segment.
        /// </summary>
        /// <returns>A string representation of the silence segment.</returns>
        public override string ToString()
        {
            return $"Silence: {StartTime:hh\\:mm\\:ss\\.fff} - {EndTime:hh\\:mm\\:ss\\.fff} ({Duration:hh\\:mm\\:ss\\.fff})";
        }
    }
}

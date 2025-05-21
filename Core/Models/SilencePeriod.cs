using System;

namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Represents a period of silence in an audio file
    /// </summary>
    public class SilencePeriod
    {
        /// <summary>
        /// Start time of the silence period
        /// </summary>
        public TimeSpan StartTime { get; set; }
        
        /// <summary>
        /// End time of the silence period
        /// </summary>
        public TimeSpan EndTime { get; set; }
        
        /// <summary>
        /// Duration of the silence period
        /// </summary>
        public TimeSpan Duration { get; set; }
    }
}

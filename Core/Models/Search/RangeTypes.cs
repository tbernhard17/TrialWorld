using System;

namespace TrialWorld.Core.Models.Search
{
    /// <summary>
    /// Represents a range between two DateTime values
    /// </summary>
    public class DateTimeRange
    {
        /// <summary>
        /// Start of the range (inclusive)
        /// </summary>
        public DateTime? Start { get; set; }

        /// <summary>
        /// End of the range (inclusive)
        /// </summary>
        public DateTime? End { get; set; }

        /// <summary>
        /// Checks if a DateTime value falls within this range
        /// </summary>
        public bool Contains(DateTime value)
        {
            if (Start == null && End == null) return true;
            if (Start == null) return value <= End;
            if (End == null) return value >= Start;
            return value >= Start && value <= End;
        }
    }

    /// <summary>
    /// Represents a range between two TimeSpan values
    /// </summary>
    public class TimeSpanRange
    {
        /// <summary>
        /// Start of the range (inclusive)
        /// </summary>
        public TimeSpan? Start { get; set; }

        /// <summary>
        /// End of the range (inclusive)
        /// </summary>
        public TimeSpan? End { get; set; }

        /// <summary>
        /// Checks if a TimeSpan value falls within this range
        /// </summary>
        public bool Contains(TimeSpan value)
        {
            if (Start == null && End == null) return true;
            if (Start == null) return value <= End;
            if (End == null) return value >= Start;
            return value >= Start && value <= End;
        }
    }
}
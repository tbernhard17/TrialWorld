using System;

namespace Core.Utilities
{
    /// <summary>
    /// Provides methods for formatting time spans and dates in human-readable format
    /// </summary>
    public static class TimeFormatter
    {
        /// <summary>
        /// Formats a TimeSpan as a human-readable string (e.g., "2 hours 30 minutes")
        /// </summary>
        /// <param name="timeSpan">TimeSpan to format</param>
        /// <param name="includeSeconds">Whether to include seconds in the output</param>
        /// <param name="includeMilliseconds">Whether to include milliseconds in the output</param>
        /// <returns>Human-readable time string</returns>
        public static string FormatTimeSpan(TimeSpan timeSpan, bool includeSeconds = true, bool includeMilliseconds = false)
        {
            if (timeSpan.TotalDays >= 1)
            {
                if (timeSpan.Hours == 0 && timeSpan.Minutes == 0)
                    return $"{timeSpan.Days} day{(timeSpan.Days != 1 ? "s" : "")}";
                    
                return $"{timeSpan.Days} day{(timeSpan.Days != 1 ? "s" : "")} {timeSpan.Hours} hour{(timeSpan.Hours != 1 ? "s" : "")}";
            }
            
            if (timeSpan.TotalHours >= 1)
            {
                return $"{timeSpan.Hours} hour{(timeSpan.Hours != 1 ? "s" : "")} {timeSpan.Minutes} minute{(timeSpan.Minutes != 1 ? "s" : "")}";
            }
            
            if (timeSpan.TotalMinutes >= 1)
            {
                string result = $"{timeSpan.Minutes} minute{(timeSpan.Minutes != 1 ? "s" : "")}";
                if (includeSeconds && timeSpan.Seconds > 0)
                    result += $" {timeSpan.Seconds} second{(timeSpan.Seconds != 1 ? "s" : "")}";
                return result;
            }
            
            if (timeSpan.TotalSeconds >= 1 || !includeMilliseconds)
            {
                return $"{timeSpan.Seconds} second{(timeSpan.Seconds != 1 ? "s" : "")}";
            }
            
            return $"{timeSpan.Milliseconds} millisecond{(timeSpan.Milliseconds != 1 ? "s" : "")}";
        }
        
        /// <summary>
        /// Formats a DateTime as a relative time string (e.g., "2 hours ago", "just now")
        /// </summary>
        /// <param name="dateTime">DateTime to format</param>
        /// <param name="referenceTime">Reference time (defaults to DateTime.Now)</param>
        /// <returns>Human-readable relative time string</returns>
        public static string FormatRelative(DateTime dateTime, DateTime? referenceTime = null)
        {
            var reference = referenceTime ?? DateTime.Now;
            
            // Handle future times
            if (dateTime > reference)
            {
                var diff = dateTime - reference;
                
                if (diff.TotalMinutes < 1)
                    return "in a moment";
                if (diff.TotalHours < 1)
                    return $"in {(int)diff.TotalMinutes} minute{((int)diff.TotalMinutes != 1 ? "s" : "")}";
                if (diff.TotalDays < 1)
                    return $"in {(int)diff.TotalHours} hour{((int)diff.TotalHours != 1 ? "s" : "")}";
                if (diff.TotalDays < 7)
                    return $"in {(int)diff.TotalDays} day{((int)diff.TotalDays != 1 ? "s" : "")}";
                if (diff.TotalDays < 30)
                    return $"in {(int)(diff.TotalDays / 7)} week{((int)(diff.TotalDays / 7) != 1 ? "s" : "")}";
                if (diff.TotalDays < 365)
                    return $"in {(int)(diff.TotalDays / 30)} month{((int)(diff.TotalDays / 30) != 1 ? "s" : "")}";
                
                return $"in {(int)(diff.TotalDays / 365)} year{((int)(diff.TotalDays / 365) != 1 ? "s" : "")}";
            }
            
            // Handle past times
            var difference = reference - dateTime;
            
            if (difference.TotalMinutes < 1)
                return "just now";
            if (difference.TotalHours < 1)
                return $"{(int)difference.TotalMinutes} minute{((int)difference.TotalMinutes != 1 ? "s" : "")} ago";
            if (difference.TotalDays < 1)
                return $"{(int)difference.TotalHours} hour{((int)difference.TotalHours != 1 ? "s" : "")} ago";
            if (difference.TotalDays < 7)
                return $"{(int)difference.TotalDays} day{((int)difference.TotalDays != 1 ? "s" : "")} ago";
            if (difference.TotalDays < 30)
                return $"{(int)(difference.TotalDays / 7)} week{((int)(difference.TotalDays / 7) != 1 ? "s" : "")} ago";
            if (difference.TotalDays < 365)
                return $"{(int)(difference.TotalDays / 30)} month{((int)(difference.TotalDays / 30) != 1 ? "s" : "")} ago";
                
            return $"{(int)(difference.TotalDays / 365)} year{((int)(difference.TotalDays / 365) != 1 ? "s" : "")} ago";
        }
        
        /// <summary>
        /// Formats a DateTime as a friendly date string with time
        /// </summary>
        /// <param name="dateTime">DateTime to format</param>
        /// <param name="includeTime">Whether to include the time</param>
        /// <returns>Formatted date string</returns>
        public static string FormatFriendlyDate(DateTime dateTime, bool includeTime = true)
        {
            var now = DateTime.Now;
            var today = now.Date;
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);
            
            string dateString;
            
            if (dateTime.Date == today)
                dateString = "Today";
            else if (dateTime.Date == yesterday)
                dateString = "Yesterday";
            else if (dateTime.Date == tomorrow)
                dateString = "Tomorrow";
            else if (dateTime.Year == now.Year)
                dateString = dateTime.ToString("MMMM d");  // e.g., "April 15"
            else
                dateString = dateTime.ToString("MMMM d, yyyy");  // e.g., "April 15, 2022"
            
            if (includeTime)
                return $"{dateString} at {dateTime.ToString("h:mm tt")}";  // e.g., "Today at 2:30 PM"
            
            return dateString;
        }
        
        /// <summary>
        /// Formats a duration in seconds as a string in the format "HH:MM:SS"
        /// </summary>
        /// <param name="seconds">Duration in seconds</param>
        /// <returns>Formatted duration string</returns>
        public static string FormatDuration(double seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            
            if (timeSpan.TotalHours >= 1)
                return timeSpan.ToString(@"h\:mm\:ss");
            
            return timeSpan.ToString(@"m\:ss");
        }
    }
}

using System;
using System.Collections.Generic;

namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Represents a chapter in media content with timing information and key points.
    /// </summary>
    public class ContentChapter
    {
        /// <summary>
        /// Gets or sets the title of the chapter.
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the start time of the chapter.
        /// </summary>
        public TimeSpan StartTime { get; set; }
        
        /// <summary>
        /// Gets or sets the end time of the chapter.
        /// </summary>
        public TimeSpan EndTime { get; set; }
        
        /// <summary>
        /// Gets or sets the summary of the chapter content.
        /// </summary>
        public string Summary { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the list of key points in the chapter.
        /// </summary>
        public List<string> KeyPoints { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets the list of speakers in the chapter.
        /// </summary>
        public List<string> Speakers { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets the sentiment of the chapter (if available).
        /// </summary>
        public string? Sentiment { get; set; }
        
        /// <summary>
        /// Gets the duration of the chapter.
        /// </summary>
        public TimeSpan Duration => EndTime - StartTime;
    }
}
using System;
using TrialWorld.Core.Models.Transcription.Interfaces;

namespace TrialWorld.Core.Models.Transcription
{
    /// <summary>
    /// Represents the sentiment analysis result for a segment of transcribed text.
    /// </summary>
    public class SentimentAnalysisResult : ISentimentAnalysisResult
    {
        /// <summary>
        /// The transcribed text for this segment.
        /// </summary>
        public string Text { get; set; } = string.Empty;
        
        /// <summary>
        /// The start time of this segment in milliseconds.
        /// </summary>
        public int Start { get; set; }
        
        /// <summary>
        /// The end time of this segment in milliseconds.
        /// </summary>
        public int End { get; set; }
        
        /// <summary>
        /// The detected sentiment for this segment. 
        /// One of: POSITIVE, NEUTRAL, NEGATIVE
        /// </summary>
        public string Sentiment { get; set; } = string.Empty;
        
        /// <summary>
        /// The confidence score for the detected sentiment, from 0 to 1.
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// Gets the start time as a TimeSpan.
        /// </summary>
        public TimeSpan StartTime => TimeSpan.FromMilliseconds(Start);
        
        /// <summary>
        /// Gets the end time as a TimeSpan.
        /// </summary>
        public TimeSpan EndTime => TimeSpan.FromMilliseconds(End);
        
        /// <summary>
        /// Gets the start time in milliseconds.
        /// </summary>
        public int StartMilliseconds => Start;
        
        /// <summary>
        /// Gets the end time in milliseconds.
        /// </summary>
        public int EndMilliseconds => End;
        
        /// <summary>
        /// Gets the confidence score for the sentiment analysis.
        /// </summary>
        public double SentimentConfidence => Confidence;
    }
}

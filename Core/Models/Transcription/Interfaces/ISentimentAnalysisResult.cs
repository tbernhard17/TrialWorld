using System;

namespace TrialWorld.Core.Models.Transcription.Interfaces
{
    /// <summary>
    /// Defines a sentiment analysis result for a segment of transcribed text.
    /// </summary>
    public interface ISentimentAnalysisResult : ITranscriptionSegment
    {
        /// <summary>
        /// Gets the detected sentiment for this segment.
        /// One of: POSITIVE, NEUTRAL, NEGATIVE
        /// </summary>
        string Sentiment { get; }
        
        /// <summary>
        /// Gets the confidence score for the detected sentiment, from 0 to 1.
        /// </summary>
        double SentimentConfidence { get; }
    }
}

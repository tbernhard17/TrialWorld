using System;
using System.Collections.Generic;

namespace TrialWorld.Core.Models.Transcription.Interfaces
{
    /// <summary>
    /// Defines the complete result of a transcription operation.
    /// </summary>
    public interface ITranscriptionResult
    {
        /// <summary>
        /// Gets the unique identifier for the transcription result.
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// Gets the transcription identifier, which may be different from the Id.
        /// </summary>
        string TranscriptId { get; }
        
        /// <summary>
        /// Gets the plain text version of the transcript.
        /// Alias for Transcript for backward compatibility.
        /// </summary>
        string Text { get; }

        /// <summary>
        /// Gets the full transcript text.
        /// </summary>
        string Transcript { get; }
        
        /// <summary>
        /// Gets the status of the transcription process.
        /// </summary>
        TranscriptionStatus Status { get; }
        
        /// <summary>
        /// Gets the error message if transcription failed.
        /// </summary>
        string Error { get; }
        
        /// <summary>
        /// Gets the confidence score for the transcription.
        /// </summary>
        double Confidence { get; }
        
        /// <summary>
        /// Gets the detected language of the transcript.
        /// </summary>
        string DetectedLanguage { get; }
        
        /// <summary>
        /// Gets the collection of transcript segments.
        /// </summary>
        IReadOnlyList<ITranscriptionSegment> Segments { get; }
        
        /// <summary>
        /// Gets the collection of sentences.
        /// </summary>
        IReadOnlyList<ITranscriptionSegment> Sentences { get; }
        
        /// <summary>
        /// Gets the collection of paragraphs.
        /// </summary>
        IReadOnlyList<ITranscriptionSegment> Paragraphs { get; }
        
        /// <summary>
        /// Gets the collection of chapters.
        /// </summary>
        IReadOnlyList<ITranscriptionSegment> Chapters { get; }
        
        /// <summary>
        /// Gets the collection of highlights.
        /// </summary>
        IReadOnlyList<ITranscriptionSegment> Highlights { get; }
        
        /// <summary>
        /// Gets the sentiment analysis results, if sentiment analysis was enabled.
        /// </summary>
        IReadOnlyList<ISentimentAnalysisResult> SentimentAnalysisResults { get; }
        
        /// <summary>
        /// Gets the timestamp when the transcription was created.
        /// </summary>
        DateTime CreatedAt { get; }
        
        /// <summary>
        /// Indicates whether the transcription was successful.
        /// </summary>
        bool IsSuccess { get; }
    }
}

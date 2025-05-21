using System;
using System.Collections.Generic;
using System.Linq;
using TrialWorld.Core.Models.Transcription.Interfaces;

namespace TrialWorld.Core.Models.Transcription
{
    /// <summary>
    /// Represents the result of a transcription operation.
    /// </summary>
    public class TranscriptionResult : ITranscriptionResult
    {
        /// <summary>
        /// The percentage of completion for the transcription (0-100). Null if not available.
        /// </summary>
        public double? PercentComplete { get; set; }

        /// <summary>
        /// Unique identifier for the transcription result.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Gets or sets the transcription identifier from the transcription service.
        /// </summary>
        public string TranscriptId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets the text representation of the transcript (alias for Transcript).
        /// </summary>
        public string Text => Transcript;

        /// <summary>
        /// Path to the media file that was transcribed.
        /// </summary>
        public string MediaPath { get; set; } = string.Empty;

        /// <summary>
        /// Path to the transcript file (e.g., SRT or JSON).
        /// </summary>
        public string TranscriptPath { get; set; } = string.Empty;

        /// <summary>
        /// The original path of the source media for this transcription.
        /// </summary>
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>
        /// Detected language of the transcript.
        /// </summary>
        public string DetectedLanguage { get; set; } = "en-US";

        /// <summary>
        /// Error message if transcription failed.
        /// </summary>
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full transcript text.
        /// </summary>
        public string Transcript { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the collection of transcript segments recognized in the transcription.
        /// </summary>
        public List<TranscriptSegment> Segments { get; set; } = new();
        
        /// <summary>
        /// Gets the collection of transcript segments as ITranscriptionSegment.
        /// </summary>
        IReadOnlyList<ITranscriptionSegment> ITranscriptionResult.Segments => Segments.Cast<ITranscriptionSegment>().ToList().AsReadOnly();

        /// <summary>
        /// Gets or sets the collection of sentences recognized in the transcription.
        /// </summary>
        public List<TranscriptSegment> Sentences { get; set; } = new();
        
        /// <summary>
        /// Gets the collection of sentences as ITranscriptionSegment.
        /// </summary>
        IReadOnlyList<ITranscriptionSegment> ITranscriptionResult.Sentences => Sentences.Cast<ITranscriptionSegment>().ToList().AsReadOnly();

        /// <summary>
        /// Gets or sets the collection of paragraphs recognized in the transcription.
        /// </summary>
        public List<TranscriptSegment> Paragraphs { get; set; } = new();
        
        /// <summary>
        /// Gets the collection of paragraphs as ITranscriptionSegment.
        /// </summary>
        IReadOnlyList<ITranscriptionSegment> ITranscriptionResult.Paragraphs => Paragraphs.Cast<ITranscriptionSegment>().ToList().AsReadOnly();

        /// <summary>
        /// List of chapters recognized in the transcription.
        /// </summary>
        public List<TranscriptSegment> Chapters { get; set; } = new();
        
        /// <summary>
        /// Gets the collection of chapters as ITranscriptionSegment.
        /// </summary>
        IReadOnlyList<ITranscriptionSegment> ITranscriptionResult.Chapters => Chapters.Cast<ITranscriptionSegment>().ToList().AsReadOnly();

        /// <summary>
        /// List of highlights recognized in the transcription.
        /// </summary>
        public List<TranscriptSegment> Highlights { get; set; } = new();
        
        /// <summary>
        /// Gets the collection of highlights as ITranscriptionSegment.
        /// </summary>
        IReadOnlyList<ITranscriptionSegment> ITranscriptionResult.Highlights => Highlights.Cast<ITranscriptionSegment>().ToList().AsReadOnly();
        
        /// <summary>
        /// Gets or sets the sentiment analysis results.
        /// </summary>
        public List<SentimentAnalysisResult> SentimentAnalysisResults { get; set; } = new();

        /// <summary>
        /// Gets or sets the total duration of the audio file.
        /// </summary>
        public TimeSpan AudioDuration { get; set; } = TimeSpan.Zero;
        
        /// <summary>
        /// Gets the sentiment analysis results as ISentimentAnalysisResult.
        /// </summary>
        IReadOnlyList<ISentimentAnalysisResult> ITranscriptionResult.SentimentAnalysisResults => 
            SentimentAnalysisResults.Cast<ISentimentAnalysisResult>().ToList().AsReadOnly();

        /// <summary>
        /// Dictionary of confidence or analysis scores by type.
        /// </summary>
        public Dictionary<string, double> Scores { get; set; } = new();

        /// <summary>
        /// Gets or sets the confidence score for the transcription.
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Gets or sets the language of the transcription.
        /// </summary>
        public string Language { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the transcription was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Gets or sets the timestamp when the transcription was completed.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Gets or sets whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Implements the IsSuccess property from ITranscriptionResult.
        /// </summary>
        bool ITranscriptionResult.IsSuccess => Success;

        /// <summary>
        /// Gets or sets the transcription identifier from the transcription service.
        /// </summary>
        string ITranscriptionResult.TranscriptId => TranscriptId;

        /// <summary>
        /// Gets the text representation of the transcript.
        /// </summary>
        string ITranscriptionResult.Text => Text;

        /// <summary>
        /// The status of the transcription process.
        /// </summary>
        public TranscriptionStatus Status { get; set; } = TranscriptionStatus.NotStarted;

        /// <summary>
        /// List of speakers detected in the transcription.
        /// </summary>
        public List<Speaker> Speakers { get; set; } = new();

        /// <summary>
        /// List of words recognized in the transcription.
        /// </summary>
        public List<Word> Words { get; set; } = new();

        /// <summary>
        /// List of timestamps for key events in the transcription.
        /// </summary>
        public List<double> Timestamps { get; set; } = new();

        /// <summary>
        /// The raw JSON response from the transcription provider.
        /// </summary>
        public string RawResponseJson { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets additional metadata for the transcription result.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();

        /// <summary>
        /// Gets or sets custom provider-specific data for the transcription result.
        /// </summary>
        public Dictionary<string, string> CustomData { get; set; } = new();
        
        // Removed duplicate SentimentAnalysisResults property - already defined above
    }
}

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrialWorld.AssemblyAIDiagnostic.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object for AssemblyAI transcription responses.
    /// </summary>
    public class TranscriptionResponseDto
    {
        /// <summary>
        /// The unique identifier of the transcript.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The language model used for the transcript.
        /// </summary>
        [JsonPropertyName("language_model")]
        public string? LanguageModel { get; set; }

        /// <summary>
        /// The acoustic model used for the transcript.
        /// </summary>
        [JsonPropertyName("acoustic_model")]
        public string? AcousticModel { get; set; }

        /// <summary>
        /// The status of the transcription.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The URL of the audio that was transcribed.
        /// </summary>
        [JsonPropertyName("audio_url")]
        public string? AudioUrl { get; set; }

        /// <summary>
        /// The text of the transcript.
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// Any error message if the transcription failed.
        /// </summary>
        [JsonPropertyName("error")]
        public string? Error { get; set; }

        /// <summary>
        /// The words in the transcript.
        /// </summary>
        [JsonPropertyName("words")]
        public List<WordDto>? Words { get; set; }

        /// <summary>
        /// The utterances in the transcript when speaker diarization is enabled.
        /// </summary>
        [JsonPropertyName("utterances")]
        public List<UtteranceDto>? Utterances { get; set; }

        /// <summary>
        /// The confidence score for the transcript.
        /// </summary>
        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        /// <summary>
        /// The duration of the audio in seconds.
        /// </summary>
        [JsonPropertyName("audio_duration")]
        public double AudioDuration { get; set; }

        /// <summary>
        /// Whether speaker diarization was enabled.
        /// </summary>
        [JsonPropertyName("speaker_labels")]
        public bool SpeakerLabels { get; set; }

        /// <summary>
        /// The results of sentiment analysis if enabled.
        /// </summary>
        [JsonPropertyName("sentiment_analysis_results")]
        public List<SentimentAnalysisResultDto>? SentimentAnalysisResults { get; set; }
    }

    /// <summary>
    /// Represents a word in the transcript.
    /// </summary>
    public class WordDto
    {
        /// <summary>
        /// The text of the word.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// The start time of the word in milliseconds.
        /// </summary>
        [JsonPropertyName("start")]
        public int Start { get; set; }

        /// <summary>
        /// The end time of the word in milliseconds.
        /// </summary>
        [JsonPropertyName("end")]
        public int End { get; set; }

        /// <summary>
        /// The confidence score for the word.
        /// </summary>
        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        /// <summary>
        /// The speaker label if speaker diarization is enabled.
        /// </summary>
        [JsonPropertyName("speaker")]
        public string? Speaker { get; set; }
    }

    /// <summary>
    /// Represents an utterance in the transcript when speaker diarization is enabled.
    /// </summary>
    public class UtteranceDto
    {
        /// <summary>
        /// The text of the utterance.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// The start time of the utterance in milliseconds.
        /// </summary>
        [JsonPropertyName("start")]
        public int Start { get; set; }

        /// <summary>
        /// The end time of the utterance in milliseconds.
        /// </summary>
        [JsonPropertyName("end")]
        public int End { get; set; }

        /// <summary>
        /// The confidence score for the utterance.
        /// </summary>
        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        /// <summary>
        /// The speaker label.
        /// </summary>
        [JsonPropertyName("speaker")]
        public string Speaker { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a sentiment analysis result.
    /// </summary>
    public class SentimentAnalysisResultDto
    {
        /// <summary>
        /// The text that was analyzed.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// The start time of the text in milliseconds.
        /// </summary>
        [JsonPropertyName("start")]
        public int Start { get; set; }

        /// <summary>
        /// The end time of the text in milliseconds.
        /// </summary>
        [JsonPropertyName("end")]
        public int End { get; set; }

        /// <summary>
        /// The sentiment of the text.
        /// </summary>
        [JsonPropertyName("sentiment")]
        public string Sentiment { get; set; } = string.Empty;

        /// <summary>
        /// The confidence score for the sentiment.
        /// </summary>
        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        /// <summary>
        /// The speaker label if speaker diarization is enabled.
        /// </summary>
        [JsonPropertyName("speaker")]
        public string? Speaker { get; set; }
    }
}

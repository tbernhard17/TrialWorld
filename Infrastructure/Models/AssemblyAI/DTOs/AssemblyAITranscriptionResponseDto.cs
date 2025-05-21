using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using TrialWorld.Core.Models.Transcription;

namespace TrialWorld.Infrastructure.Models.AssemblyAI.DTOs
{
    /// <summary>
    /// INTERNAL: Used only for API transport/serialization. Do not use in domain or business logic.
    /// </summary>
    /// <summary>
    /// Represents an AssemblyAI transcription response according to their latest API.
    /// This is a DTO (Data Transfer Object) that directly maps to the JSON response.
    /// </summary>
    public class AssemblyAITranscriptionResponseDto
    {
        /// <summary>
        /// The unique identifier for the transcription
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// The status of the transcription
        /// </summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        /// <summary>
        /// The URL of the audio file that was transcribed
        /// </summary>
        [JsonPropertyName("audio_url")]
        public string? AudioUrl { get; set; }

        /// <summary>
        /// The full text of the transcription
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// The confidence score of the transcription
        /// </summary>
        [JsonPropertyName("confidence")]
        public double? Confidence { get; set; }

        /// <summary>
        /// The language of the transcription
        /// </summary>
        [JsonPropertyName("language_code")]
        public string? Language { get; set; }

        /// <summary>
        /// The audio duration in seconds
        /// </summary>
        [JsonPropertyName("audio_duration")]
        public double? AudioDuration { get; set; }

        /// <summary>
        /// The utterances in the transcription (speaker segments)
        /// </summary>
        [JsonPropertyName("utterances")]
        public List<UtteranceDto>? Utterances { get; set; }

        /// <summary>
        /// The words in the transcription
        /// </summary>
        [JsonPropertyName("words")]
        public List<Word>? Words { get; set; }

        /// <summary>
        /// The sentiment analysis results
        /// </summary>
        [JsonPropertyName("sentiment_analysis_results")]
        public List<SentimentResultDto>? SentimentResults { get; set; }

        /// <summary>
        /// The error message if the transcription failed
        /// </summary>
        [JsonPropertyName("error")]
        public string? Error { get; set; }

        /// <summary>
        /// The timestamp when the transcription was created
        /// </summary>
        [JsonPropertyName("created")]
        public DateTime? Created { get; set; }

        /// <summary>
        /// The timestamp when the transcription was completed
        /// </summary>
        [JsonPropertyName("completed")]
        public DateTime? Completed { get; set; }

        /// <summary>
        /// The percentage of completion for the transcription (0-100)
        /// </summary>
        [JsonPropertyName("percent_complete")]
        public double? PercentComplete { get; set; }
    }
}

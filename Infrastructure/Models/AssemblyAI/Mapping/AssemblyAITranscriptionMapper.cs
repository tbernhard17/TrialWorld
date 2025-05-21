using TrialWorld.Core.Models.Transcription;
using TrialWorld.Infrastructure.Models.AssemblyAI.DTOs;
using TrialWorld.Infrastructure.Transcription.DTOs;

namespace TrialWorld.Infrastructure.Models.AssemblyAI.Mapping
{
    /// <summary>
    /// Central mapping utility for converting between AssemblyAI DTOs and Core transcription models.
    /// </summary>
    public static class AssemblyAITranscriptionMapper
    {
        public static TranscriptionResult ToCore(AssemblyAITranscriptionResponseDto dto)
        {
            return new TranscriptionResult
            {
                Id = dto.Id ?? string.Empty,
                Transcript = dto.Text ?? string.Empty,
                DetectedLanguage = dto.Language ?? "en-US",
                Error = dto.Error ?? string.Empty,
                PercentComplete = dto.PercentComplete,
                // TODO: Map additional fields as needed
            };
        }

        /// <summary>
        /// Converts a Core TranscriptionConfig to a DTO for API requests.
        /// </summary>
        /// <param name="config">The core configuration object.</param>
        /// <param name="audioUrl">The URL of the audio file to transcribe.</param>
        /// <returns>A TranscriptionRequestDto configured for the AssemblyAI API.</returns>
        public static TranscriptionRequestDto ToDto(TranscriptionConfig config, string audioUrl)
        {
            return new TranscriptionRequestDto
            {
                AudioUrl = audioUrl,
                SpeakerLabels = config.EnableSpeakerDiarization,
                SentimentAnalysis = config.EnableSentimentAnalysis,
                LanguageCode = config.LanguageCode,
                WebhookUrl = config.WebhookUrl,
                WebhookAuthHeaderName = config.WebhookAuthHeaderName,
                WebhookAuthHeaderValue = config.WebhookAuthHeaderValue,
                // Set default values for required properties
                Punctuate = true,
                FormatText = true
            };
        }

        public static SentimentAnalysisResult ToCore(SentimentResultDto dto)
        {
            return new SentimentAnalysisResult
            {
                Text = dto.Text ?? string.Empty,
                Start = dto.Start,
                End = dto.End,
                Sentiment = dto.Sentiment.ToString(),
                Confidence = dto.Confidence
            };
        }
    }
}

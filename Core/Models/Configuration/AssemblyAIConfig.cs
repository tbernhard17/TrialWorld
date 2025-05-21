using System;

namespace TrialWorld.Core.Models.Configuration
{
    /// <summary>
    /// Configuration settings for AssemblyAI services.
    /// </summary>
    public class AssemblyAIConfig
    {
        /// <summary>
        /// The API key for AssemblyAI services.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// The base URL for AssemblyAI API.
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.assemblyai.com/v2";

        /// <summary>
        /// The timeout in seconds for API requests.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// The maximum number of retry attempts for failed API requests.
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// The delay between retry attempts in seconds.
        /// </summary>
        public int RetryDelaySeconds { get; set; } = 2;

        /// <summary>
        /// Whether to enable automatic punctuation in transcriptions.
        /// </summary>
        public bool EnablePunctuation { get; set; } = true;

        /// <summary>
        /// Whether to enable speaker diarization in transcriptions.
        /// </summary>
        public bool EnableSpeakerDiarization { get; set; } = true;

        /// <summary>
        /// Whether to enable sentiment analysis in transcriptions.
        /// </summary>
        public bool EnableSentimentAnalysis { get; set; } = false;

        /// <summary>
        /// Whether to enable entity detection in transcriptions.
        /// </summary>
        public bool EnableEntityDetection { get; set; } = false;
    }
}

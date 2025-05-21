using System;
using System.ComponentModel.DataAnnotations;

namespace TrialWorld.Infrastructure.Transcription.Configuration
{
    /// <summary>
    /// Configuration options for the AssemblyAI API integration.
    /// </summary>
    public class AssemblyAIOptions
    {
        /// <summary>
        /// Gets or sets the API key for the AssemblyAI API.
        /// </summary>
        [Required]
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the base URL for the AssemblyAI API.
        /// </summary>
        [Required]
        [Url]
        public string BaseUrl { get; set; } = "https://api.assemblyai.com/v2";

        /// <summary>
        /// Gets or sets the polling interval in milliseconds for checking transcription status.
        /// </summary>
        [Range(1000, 60000)]
        public int PollingIntervalMs { get; set; } = 5000;

        /// <summary>
        /// Gets or sets the maximum number of retry attempts for API requests.
        /// </summary>
        [Range(0, 10)]
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Gets or sets the timeout in seconds for API requests.
        /// </summary>
        [Range(1, 3600)]
        public int TimeoutSeconds { get; set; } = 30;
        
        /// <summary>
        /// Gets or sets the chunk size in bytes for uploading files.
        /// </summary>
        [Range(1048576, 10485760)] // 1MB to 10MB
        public int UploadChunkSizeBytes { get; set; } = 5242880; // 5MB default
        
        /// <summary>
        /// Gets or sets a value indicating whether detailed logging is enabled.
        /// </summary>
        public bool EnableDetailedLogging { get; set; } = true;
        
        /// <summary>
        /// Gets or sets a value indicating whether to use the official SDK.
        /// </summary>
        public bool UseSDK { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the exponent for calculating retry backoff.
        /// </summary>
        [Range(1, 5)]
        public int RetryBackoffExponent { get; set; } = 2;
        
        /// <summary>
        /// Gets or sets the initial retry delay in milliseconds.
        /// </summary>
        [Range(100, 10000)]
        public int InitialRetryDelayMs { get; set; } = 1000;
        
        /// <summary>
        /// Gets or sets the maximum retry delay in milliseconds.
        /// </summary>
        [Range(1000, 300000)]
        public int MaxRetryDelayMs { get; set; } = 30000;
        
        /// <summary>
        /// Gets or sets the failure threshold for the circuit breaker.
        /// </summary>
        [Range(1, 100)]
        public int CircuitBreakerFailureThreshold { get; set; } = 5;
        
        /// <summary>
        /// Gets or sets the sampling duration in seconds for the circuit breaker.
        /// </summary>
        [Range(1, 300)]
        public int CircuitBreakerSamplingDurationSeconds { get; set; } = 30;
        
        /// <summary>
        /// Gets or sets the duration in seconds for the circuit breaker to stay open.
        /// </summary>
        [Range(1, 300)]
        public int CircuitBreakerDurationSeconds { get; set; } = 60;
        
        /// <summary>
        /// Gets or sets the timeout in seconds for individual API requests.
        /// </summary>
        [Range(1, 300)]
        public int RequestTimeoutSeconds { get; set; } = 30;
        
        /// <summary>
        /// Gets or sets the path to the FFmpeg executable.
        /// </summary>
        public string? FFmpegPath { get; set; }
        
        /// <summary>
        /// Gets or sets the path to the transcription database.
        /// </summary>
        public string? TranscriptionDatabasePath { get; set; }
    }
}

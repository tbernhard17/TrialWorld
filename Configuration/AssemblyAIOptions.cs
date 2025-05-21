using System;

namespace TrialWorld.AssemblyAIDiagnostic.Configuration
{
    /// <summary>
    /// Configuration options for the AssemblyAI API integration.
    /// </summary>
    public class AssemblyAIOptions
    {
        /// <summary>
        /// The API key for authenticating with the AssemblyAI API.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;
        
        /// <summary>
        /// The base URL for the AssemblyAI API.
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.assemblyai.com/v2";
        
        /// <summary>
        /// The interval in milliseconds between polling attempts for transcription status.
        /// </summary>
        public int PollingIntervalMs { get; set; } = 5000;
        
        /// <summary>
        /// The maximum number of retry attempts for transient failures.
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;
        
        /// <summary>
        /// The timeout in seconds for API requests.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;
    }
}

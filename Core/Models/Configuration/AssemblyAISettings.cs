namespace TrialWorld.Core.Models.Configuration
{
    /// <summary>
    /// Represents the available AI models offered by AssemblyAI
    /// </summary>
    public enum AssemblyAIModel
    {
        /// <summary>
        /// Nova model (previously called Nano) - fastest and most affordable option
        /// Maps to "nova" in the API
        /// </summary>
        Nova,
        
        /// <summary>
        /// Universal model - balanced speed and accuracy for most use cases
        /// Maps to "universal" in the API
        /// </summary>
        Universal,
        
        /// <summary>
        /// SLAM-1 model - highest accuracy and best performance for complex audio
        /// Maps to "slam-1" in the API
        /// </summary>
        Slam1
    }
    
    /// <summary>
    /// Configuration settings for the AssemblyAI transcription service
    /// </summary>
    public class AssemblyAISettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.assemblyai.com/v2";
        public int TimeoutSeconds { get; set; } = 600;
        public int PollingIntervalSeconds { get; set; } = 5;
        public int UploadChunkSizeBytes { get; set; } = 5242880;
        public bool EnableDetailedLogging { get; set; } = false;
        
        /// <summary>
        /// The AI model to use for transcription (affects accuracy and cost)
        /// </summary>
        public AssemblyAIModel Model { get; set; } = AssemblyAIModel.Universal;
        
        /// <summary>
        /// Determines whether to use auto_chapters feature instead of summarization
        /// (AssemblyAI only allows one of these features at a time)
        /// </summary>
        public bool PreferChaptersOverSummary { get; set; } = true;
        
        /// <summary>
        /// Gets the model string to use in API requests based on the selected model
        /// </summary>
        public string GetModelStringForApi()
        {
            return Model switch
            {
                AssemblyAIModel.Nova => "nova",
                AssemblyAIModel.Universal => "universal",
                AssemblyAIModel.Slam1 => "slam-1",
                _ => "universal" // Default to universal if unknown
            };
        }
    }
}
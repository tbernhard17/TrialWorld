namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Represents options for processing media.
    /// </summary>
    public class CoreProcessingOptions
    {
        public string InputPath { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public bool PreserveQuality { get; set; } = true;
        public Dictionary<string, string> AdditionalParameters { get; set; } = new();
    }
}
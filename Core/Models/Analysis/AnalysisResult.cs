
namespace TrialWorld.Core.Models.Analysis
{
    /// <summary>
    /// Represents the result of a media analysis operation.
    /// </summary>
    public class AnalysisResult
    {
        public string Summary { get; init; } = string.Empty;
        public Dictionary<string, string> Metadata { get; init; } = new();
        public AnalysisStatus Status { get; set; } = AnalysisStatus.NotStarted;
    }
}

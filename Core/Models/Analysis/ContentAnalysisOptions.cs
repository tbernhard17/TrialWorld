
namespace TrialWorld.Core.Models.Analysis
{
    /// <summary>
    /// Specialized content analysis options for advanced scenarios.
    /// </summary>
    public class ContentAnalysisOptions
    {
        public bool DetectFaces { get; init; } = true;
        public bool DetectObjects { get; init; } = false;
        public bool DetectSpeech { get; init; } = true;
    }
}

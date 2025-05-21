
namespace TrialWorld.Core.Models.Analysis
{
    /// <summary>
    /// Options for customizing media analysis or processing behavior.
    /// </summary>
    public class AnalysisOptions
    {
        public bool EnableAudioAnalysis { get; init; } = true;
        public bool EnableVideoAnalysis { get; init; } = true;
        public bool ExtractMetadata { get; init; } = true;
        public bool PerformEnhancement { get; init; } = false;
    }
}

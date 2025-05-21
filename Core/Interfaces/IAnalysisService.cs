using System.Threading.Tasks;
using TrialWorld.Core.Models.Analysis; // Assuming result models are here

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Placeholder interface for content analysis services.
    /// </summary>
    public interface IAnalysisService
    {
        // Define methods based on usage, e.g.:
        Task<AnalysisResult> AnalyzeAsync(string transcript, CancellationToken cancellationToken = default);
    }

    // Placeholder result model
    public class AnalysisResult
    {
        public bool Success { get; set; } = true;
        public string Summary { get; set; } = "Analysis complete.";
        // Add other analysis fields (keywords, sentiment, etc.)
    }
} 
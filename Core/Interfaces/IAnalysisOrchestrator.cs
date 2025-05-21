using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models.Analysis;

namespace TrialWorld.Core.Interfaces
{
    public interface IAnalysisOrchestrator
    {
        Task<AnalysisResult> AnalyzeMediaAsync(string mediaPath, AnalysisOptions options, CancellationToken cancellationToken = default);
        Task<AnalysisStatus> GetAnalysisStatusAsync(string analysisId, CancellationToken cancellationToken = default);
        Task<bool> CancelAnalysisAsync(string analysisId, CancellationToken cancellationToken = default);
    }
}
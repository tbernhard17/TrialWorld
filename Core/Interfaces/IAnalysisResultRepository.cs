using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrialWorld.Core.Interfaces // Assuming this is the correct namespace for Core interfaces
{
    /// <summary>
    /// Generic repository interface for accessing analysis results associated with media.
    /// </summary>
    /// <typeparam name="TAnalysisResult">The type of the analysis result entity.</typeparam>
    public interface IAnalysisResultRepository<TAnalysisResult> where TAnalysisResult : class
    {
        /// <summary>
        /// Gets all analysis results associated with a specific media ID.
        /// </summary>
        /// <param name="mediaId">The unique identifier of the media.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of analysis results.</returns>
        Task<IEnumerable<TAnalysisResult>> GetByMediaIdAsync(Guid mediaId);

        /// <summary>
        /// Adds a collection of analysis results to the repository.
        /// </summary>
        /// <param name="results">The collection of analysis results to add.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task AddRangeAsync(IEnumerable<TAnalysisResult> results);

        /// <summary>
        /// Deletes all analysis results associated with a specific media ID.
        /// </summary>
        /// <param name="mediaId">The unique identifier of the media whose results should be deleted.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteByMediaIdAsync(Guid mediaId);
    }
} 
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Search;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Interface for search indexing operations.
    /// </summary>
    public interface ISearchIndexer
    {
        /// <summary>
        /// Gets the current processing status of the search indexer.
        /// </summary>
        /// <returns>The current processing status.</returns>
        Task<Models.Search.SearchProcessingStatus> GetProcessingStatusAsync();
        /// <summary>
        /// Adds or updates content in the search index.
        /// </summary>
        Task<bool> IndexAsync(string mediaId, SearchableContent content, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs a search using a pre-built Lucene Query object.
        /// </summary>
        /// <param name="query">The Lucene Query object.</param>
        /// <param name="maxResults">Maximum number of results to return.</param>
        /// <param name="skipResults">Number of results to skip (for pagination).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of searchable content results.</returns>
        Task<List<SearchableContent>> SearchAsync(object query, int maxResults, int skipResults = 0, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes content associated with a media ID from the index.
        /// </summary>
        Task<bool> DeleteAsync(string mediaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates content in the search index (effectively calls IndexAsync).
        /// </summary>
        Task<bool> UpdateAsync(string mediaId, SearchableContent content, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets facet counts for a specific field.
        /// </summary>
        /// <param name="facetField">The field to get facet counts for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A dictionary of facet counts.</returns>
        Task<Dictionary<string, int>> GetFacetsAsync(string facetField, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes all documents from the search index.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        Task<bool> ClearAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets basic statistics about the search index.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An object containing index statistics.</returns>
        Task<IndexStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Event that is raised when the processing status changes.
        /// </summary>
        event EventHandler<Models.Search.SearchProcessingStatus> ProcessingStatusChanged;
    }
}
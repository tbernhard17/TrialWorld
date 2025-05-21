using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Repository interface for managing user feedback on search results.
    /// </summary>
    public interface ISearchFeedbackRepository
    {
        /// <summary>
        /// Adds a new feedback entry to the repository.
        /// </summary>
        /// <param name="feedback">The feedback object to add.</param>
        /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddFeedbackAsync(SearchFeedback feedback, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves feedback entries submitted by a specific user.
        /// </summary>
        /// <param name="userId">The identifier of the user whose feedback is to be retrieved.</param>
        /// <param name="limit">The maximum number of feedback entries to return.</param>
        /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, yielding a collection of feedback entries.</returns>
        Task<IEnumerable<SearchFeedback>> GetFeedbackForUserAsync(string userId, int limit = 1000, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the most recent feedback entries submitted across all users.
        /// </summary>
        /// <param name="limit">The maximum number of feedback entries to return.</param>
        /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, yielding a collection of the most recent feedback entries.</returns>
        Task<IEnumerable<SearchFeedback>> GetRecentFeedbackAsync(int limit = 1000, CancellationToken cancellationToken = default);
    }
} 
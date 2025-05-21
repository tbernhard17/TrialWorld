using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Persistence.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Persistence.Entities;

namespace TrialWorld.Persistence.Repositories
{
    /// <summary>
    /// EF Core implementation for storing and retrieving search feedback.
    /// </summary>
    public class SearchFeedbackRepository : ISearchFeedbackRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SearchFeedbackRepository> _logger;
        // TODO: Add IMapper dependency if using AutoMapper

        public SearchFeedbackRepository(AppDbContext context, ILogger<SearchFeedbackRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddFeedbackAsync(SearchFeedback feedback, CancellationToken cancellationToken = default)
        {
            if (feedback == null) throw new ArgumentNullException(nameof(feedback));

            _logger.LogInformation("Adding search feedback for query: {Query}", feedback.Query);
            try
            {
                // Map Core model to entity
                var entity = new SearchFeedbackEntity
                {
                    Id = Guid.NewGuid(), // Generate new Guid
                    Query = feedback.Query,
                    Feedback = feedback.Feedback,
                    Timestamp = feedback.Timestamp,
                    UserId = feedback.UserId // Assuming UserId added to Core model
                };
                // Use SearchFeedbackEntities DbSet
                _context.SearchFeedbackEntities.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully added search feedback with ID: {Id}", entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add search feedback.");
                throw;
            }
        }

        public async Task<IEnumerable<SearchFeedback>> GetFeedbackForUserAsync(string userId, int limit = 1000, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID cannot be empty", nameof(userId));
            if (limit <= 0) limit = 1000;

            _logger.LogDebug("Getting feedback for user {UserId} with limit {Limit}", userId, limit);
            try
            {
                // Use SearchFeedbackEntities DbSet
                var entities = await _context.SearchFeedbackEntities
                                            .Where(f => f.UserId == userId)
                                            .OrderByDescending(f => f.Timestamp)
                                            .Take(limit)
                                            .ToListAsync(cancellationToken);
                // Map entities back to Core model
                return entities.Select(MapToCoreModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get feedback for user {UserId}", userId);
                return Enumerable.Empty<SearchFeedback>();
            }
        }

        public async Task<IEnumerable<SearchFeedback>> GetRecentFeedbackAsync(int limit = 1000, CancellationToken cancellationToken = default)
        {
            if (limit <= 0) limit = 1000;

            _logger.LogDebug("Getting recent feedback with limit {Limit}", limit);
            try
            {
                // Use SearchFeedbackEntities DbSet
                var entities = await _context.SearchFeedbackEntities
                                            .OrderByDescending(f => f.Timestamp)
                                            .Take(limit)
                                            .ToListAsync(cancellationToken);
                // Map entities back to Core model
                return entities.Select(MapToCoreModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get recent feedback.");
                return Enumerable.Empty<SearchFeedback>();
            }
        }

        /// <summary>
        /// Maps a SearchFeedbackEntity to a SearchFeedback core model.
        /// </summary>
        /// <param name="entity">The entity to map.</param>
        /// <returns>The mapped core model.</returns>
        private SearchFeedback MapToCoreModel(SearchFeedbackEntity entity)
        {
            return new SearchFeedback 
            { 
                Id = entity.Id, // Now using Guid for Id
                Query = entity.Query,
                Feedback = entity.Feedback,
                Timestamp = entity.Timestamp,
                UserId = entity.UserId 
            };
        }
    }
}
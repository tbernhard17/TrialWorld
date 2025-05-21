using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Interfaces;
using TrialWorld.Persistence.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TrialWorld.Persistence.Repositories
{
    /// <summary>
    /// Generic EF Core repository implementation for analysis results.
    /// Assumes TAnalysisResult has a Guid MediaId property for filtering.
    /// Adheres to Rule #2 (Implementations in Infrastructure/Persistence).
    /// Adheres to Rule #7 (Inject Logger).
    /// </summary>
    /// <typeparam name="TAnalysisResult">The type of analysis result entity.</typeparam>
    public class AnalysisResultRepository<TAnalysisResult> : IAnalysisResultRepository<TAnalysisResult> where TAnalysisResult : class
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AnalysisResultRepository<TAnalysisResult>> _logger;
        private readonly DbSet<TAnalysisResult> _dbSet;

        public AnalysisResultRepository(AppDbContext context, ILogger<AnalysisResultRepository<TAnalysisResult>> logger)
        {
            _context = context;
            _logger = logger;
            _dbSet = _context.Set<TAnalysisResult>();
        }

        public async Task<IEnumerable<TAnalysisResult>> GetByMediaIdAsync(Guid mediaId)
        {
            _logger.LogInformation("Fetching {AnalysisType} results by MediaId: {MediaId}", typeof(TAnalysisResult).Name, mediaId);
            try
            {
                // Using expression trees for dynamic filtering based on assumed "MediaId" property.
                // Constraining TAnalysisResult with IHasMediaId is safer.
                var parameter = Expression.Parameter(typeof(TAnalysisResult), "e");
                var property = Expression.Property(parameter, "MediaId"); // ASSUMES MediaId property exists
                var constant = Expression.Constant(mediaId);
                var equality = Expression.Equal(property, constant);
                var lambda = Expression.Lambda<Func<TAnalysisResult, bool>>(equality, parameter);

                return await _dbSet.Where(lambda).AsNoTracking().ToListAsync();
            }
            catch (Exception ex) // Catch potential property access errors or DB errors
            {
                _logger.LogError(ex, "Error fetching {AnalysisType} results by MediaId: {MediaId}", typeof(TAnalysisResult).Name, mediaId);
                throw;
            }
        }

        public async Task AddRangeAsync(IEnumerable<TAnalysisResult> results)
        {
            if (results == null || !results.Any())
            {
                _logger.LogWarning("Attempted to add an empty or null collection of {AnalysisType} results.", typeof(TAnalysisResult).Name);
                return;
            }

            _logger.LogInformation("Adding {ResultCount} {AnalysisType} results.", results.Count(), typeof(TAnalysisResult).Name);
            try
            {
                await _dbSet.AddRangeAsync(results);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error adding range of {AnalysisType} results.", typeof(TAnalysisResult).Name);
                throw;
            }
        }

        public async Task DeleteByMediaIdAsync(Guid mediaId)
        {
            _logger.LogInformation("Deleting all {AnalysisType} results for MediaId: {MediaId}", typeof(TAnalysisResult).Name, mediaId);
            try
            {
                // Using expression trees for dynamic filtering.
                var parameter = Expression.Parameter(typeof(TAnalysisResult), "e");
                var property = Expression.Property(parameter, "MediaId"); // ASSUMES MediaId property exists
                var constant = Expression.Constant(mediaId);
                var equality = Expression.Equal(property, constant);
                var lambda = Expression.Lambda<Func<TAnalysisResult, bool>>(equality, parameter);

                // EF Core 7+ recommended way:
                // var deletedCount = await _dbSet.Where(lambda).ExecuteDeleteAsync(); 
                // _logger.LogInformation("Deleted {ResultCount} {AnalysisType} results...", deletedCount, ...);

                // Fallback for older EF Core versions or complex logic
                var resultsToDelete = await _dbSet.Where(lambda).ToListAsync();
                if (resultsToDelete.Any())
                {
                    _dbSet.RemoveRange(resultsToDelete);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Deleted {ResultCount} {AnalysisType} results for MediaId: {MediaId}", resultsToDelete.Count, typeof(TAnalysisResult).Name, mediaId);
                }
                else
                {
                    _logger.LogWarning("No {AnalysisType} results found to delete for MediaId: {MediaId}", typeof(TAnalysisResult).Name, mediaId);
                }
            }
            catch (Exception ex) // Catch potential property access errors or DB errors
            {
                _logger.LogError(ex, "Error deleting {AnalysisType} results by MediaId: {MediaId}", typeof(TAnalysisResult).Name, mediaId);
                throw;
            }
        }

        // Add other specific query methods relevant to TAnalysisResult...
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Models.Transcription;
using TrialWorld.Core.Services;
using TrialWorld.Presentation.Models;

namespace TrialWorld.Presentation.ViewModels.Search
{
    /// <summary>
    /// Service for searching transcriptions
    /// </summary>
    public class SearchService
    {
        private readonly ITranscriptionDatabaseService _transcriptionDatabaseService;
        private readonly ILogger<SearchService> _logger;
        
        /// <summary>
        /// Initializes a new instance of the SearchService class
        /// </summary>
        /// <param name="transcriptionDatabaseService">The transcription database service</param>
        /// <param name="logger">The logger</param>
        public SearchService(
            ITranscriptionDatabaseService transcriptionDatabaseService,
            ILogger<SearchService> logger)
        {
            _transcriptionDatabaseService = transcriptionDatabaseService ?? throw new ArgumentNullException(nameof(transcriptionDatabaseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Searches for transcriptions containing the specified query
        /// </summary>
        /// <param name="query">The search query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The search results</returns>
        public async Task<IEnumerable<SearchResultItem>> SearchAsync(string query, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Enumerable.Empty<SearchResultItem>();
            }
            
            try
            {
                _logger.LogInformation("Searching for: {Query}", query);
                
                var transcriptions = await _transcriptionDatabaseService.SearchTranscriptionsAsync(query, cancellationToken);
                
                var results = transcriptions.Select(t => new SearchResultItem
                {
                    TranscriptId = t.Id,
                    Text = t.Transcript ?? string.Empty,
                    Confidence = t.Confidence,
                    Language = t.Language ?? "en",
                    CreatedAt = t.CreatedAt
                }).ToList();
                
                _logger.LogInformation("Found {Count} results for query: {Query}", results.Count, query);
                
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for query: {Query}", query);
                return Enumerable.Empty<SearchResultItem>();
            }
        }
    }
}

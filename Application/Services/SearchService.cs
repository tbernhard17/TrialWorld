using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrialWorld.Application.Models;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models.Search;
using TrialWorld.Core.Models;

namespace TrialWorld.Application.Services
{
    public class AppSearchService
    {
        private readonly ISearchIndexer _searchIndexer;

        private readonly ILogger<AppSearchService> _logger;
        private readonly ConcurrentDictionary<string, UserPreferencesDto> _userPreferences;

        public AppSearchService(
            ISearchIndexer searchIndexer,
            ILogger<AppSearchService> logger)
        {
            _searchIndexer = searchIndexer ?? throw new ArgumentNullException(nameof(searchIndexer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userPreferences = new ConcurrentDictionary<string, UserPreferencesDto>();
        }

        public async Task<SearchResults> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                _logger.LogError("SearchAsync received a null query.");
                return new SearchResults { Items = new List<SearchResultItem>() };
            }
            
            _logger.LogInformation("Executing search for query: {Query}", query.Text ?? "[Empty]");

            var indexerQuery = query;
            var indexResults = await _searchIndexer.SearchAsync(indexerQuery.Text ?? string.Empty, indexerQuery.PageSize, (indexerQuery.PageNumber - 1) * indexerQuery.PageSize, cancellationToken);

            if (indexResults == null || !indexResults.Any())
            {
                _logger.LogWarning("No search results returned from indexer for query: {Query}", query?.Text);
                return new SearchResults { Items = new List<SearchResultItem>() };
            }

            var userPrefsDto = new UserPreferencesDto { /* Default Prefs - UserId cannot be obtained from query */ };
            var userPrefsCore = MapDtoToCorePrefs(userPrefsDto);

            var finalResults = indexResults.Select(MapContentToResultItem).ToList();

            var searchResults = new SearchResults
            {
                Items = finalResults,
                TotalMatches = finalResults.Count, // TODO: Get actual total count from indexer if possible
                ElapsedMilliseconds = 0 // TODO: Add timing
            };

            _logger.LogInformation("Search completed with {Count} results", searchResults.TotalMatches);
            return searchResults;
        }

        public void UpdateUserPreferences(string userId, UserPreferencesDto preferences)
        {
            _userPreferences.AddOrUpdate(userId, preferences, (_, _) => preferences);
            _logger.LogInformation("User preferences updated for user {UserId}", userId);
        }

        private SearchResultItem MapContentToResultItem(SearchableContent content)
        {
            return new SearchResultItem
            {
                Id = content.Id,
                Title = content.Title ?? "Untitled",
                MediaPath = content.FilePath,
                MediaType = "unknown",
                DurationInSeconds = content.Duration.TotalSeconds,
                FileDate = content.CreatedAt,
                Score = content.Score,
                MatchTimestamps = new List<MatchTimestamp>(),
                TextMatches = new List<TextMatch>(),
                Speakers = new List<SpeakerSummary>(),
                Topics = content.Topics ?? new List<string>()
            };
        }

        private TrialWorld.Core.Models.UserPreferences MapDtoToCorePrefs(UserPreferencesDto dto)
        {
            return new TrialWorld.Core.Models.UserPreferences { /* map other fields if they exist */ };
        }
    }
}

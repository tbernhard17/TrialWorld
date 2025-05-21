using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TrialWorld.Contracts;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Search;
using TrialWorld.Infrastructure.Search;
using TrialWorld.Application.Models;

namespace TrialWorld.WebApi.Controllers
{
    /// <summary>
    /// Controller for search operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ISearchIndexer _searchIndexer;
        private readonly ILogger<SearchController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchController"/> class
        /// </summary>
        /// <param name="searchService">Search service</param>
        /// <param name="searchIndexer">Search indexer</param>
        /// <param name="logger">Logger</param>
        public SearchController(
            ISearchService searchService,
            ISearchIndexer searchIndexer,
            ILogger<SearchController> logger)
        {
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
            _searchIndexer = searchIndexer ?? throw new ArgumentNullException(nameof(searchIndexer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Performs a basic search across all content
        /// </summary>
        /// <param name="query">The search query</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of results per page</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Search results</returns>
        [HttpGet]
        [ProducesResponseType(typeof(SearchResultsDto), 200)]
        public async Task<IActionResult> Search(
            [FromQuery] string query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Search request received for query: {Query}", query);

                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest("Search query cannot be empty");
                }

                var searchQuery = new SearchQuery
                {
                    Text = query,
                    PageNumber = page,
                    PageSize = pageSize,
                    Filters = new Dictionary<string, string>(),
                    SortBy = ""
                };

                var results = await _searchService.SearchAsync(searchQuery, cancellationToken);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing search for query: {Query}", query);
                return StatusCode(500, "An error occurred while processing your search request");
            }
        }

        /// <summary>
        /// Performs a semantic search based on a concept
        /// </summary>
        /// <param name="concept">The concept to search for</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of results per page</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Search results</returns>
        [HttpGet("semantic")]
        [ProducesResponseType(typeof(SearchResultsDto), 200)]
        public async Task<IActionResult> SemanticSearch(
            [FromQuery] string concept,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Semantic search request received for concept: {Concept}", concept);

                if (string.IsNullOrWhiteSpace(concept))
                {
                    return BadRequest("Search concept cannot be empty");
                }

                var results = await _searchService.SemanticSearchAsync(
                    concept,
                    null,
                    page,
                    pageSize,
                    cancellationToken);

                // Convert to DTO format
                var resultsDto = MapToSearchResultsDto(concept, results);

                return Ok(resultsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing semantic search for concept: {Concept}", concept);
                return StatusCode(500, "An error occurred while processing your semantic search request");
            }
        }

        /// <summary>
        /// Searches for media containing specific emotions
        /// </summary>
        /// <param name="emotion">The emotion to search for</param>
        /// <param name="minIntensity">Minimum emotion intensity (0.0-1.0)</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of results per page</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Search results</returns>
        [HttpGet("emotions/{emotion}")]
        [ProducesResponseType(typeof(SearchResultsDto), 200)]
        public async Task<IActionResult> SearchByEmotion(
            string emotion,
            [FromQuery] double minIntensity = 0.6,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Emotion search request received for emotion: {Emotion}", emotion);

                var searchQuery = new SearchQuery
                {
                    Text = $"emotion:{emotion}",
                    PageNumber = page,
                    PageSize = pageSize,
                    Filters = new Dictionary<string, string>
                    {
                        { "emotion", emotion },
                        { "minEmotionScore", minIntensity.ToString(System.Globalization.CultureInfo.InvariantCulture) }
                    },
                    SortBy = ""
                };

                var results = await _searchService.SearchAsync(searchQuery, cancellationToken);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching by emotion: {Emotion}", emotion);
                return StatusCode(500, "An error occurred while processing your emotion search request");
            }
        }

        /// <summary>
        /// Gets facet values for filtering search results
        /// </summary>
        /// <param name="facetField">The field to get facets for</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Facet values and counts</returns>
        [HttpGet("facets/{facetField}")]
        [ProducesResponseType(typeof(Dictionary<string, int>), 200)]
        public async Task<IActionResult> GetFacets(
            string facetField,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Facet request received for field: {FacetField}", facetField);

                var facets = await _searchIndexer.GetFacetsAsync(facetField);
                return Ok(facets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facets for field: {FacetField}", facetField);
                return StatusCode(500, "An error occurred while retrieving facets");
            }
        }

        /// <summary>
        /// Rebuilds the search index
        /// </summary>
        /// <returns>Result of the index rebuild operation</returns>
        [HttpPost("index/rebuild")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> RebuildIndex()
        {
            try
            {
                _logger.LogInformation("Index rebuild request received");

                // This would typically be a long-running operation
                // For a real implementation, consider using a background service or queue

                // Return accepted response with a job ID
                return Accepted("index-rebuild-job-123");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebuilding search index");
                return StatusCode(500, "An error occurred while rebuilding the search index");
            }
        }

        /// <summary>
        /// Updates user search preferences for personalized results
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="preferences">User preferences</param>
        /// <returns>Result of the update operation</returns>
        [HttpPost("preferences/{userId}")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<IActionResult> UpdatePreferences(
            string userId,
            [FromBody] UserPreferencesDto preferences)
        {
            try
            {
                _logger.LogInformation("Update preferences request received for user: {UserId}", userId);

                // Convert DTO to domain model and update
                var userPrefs = MapToUserPreferences(preferences);

                // TODO: Update Application/Core layer service to accept UserPreferences object
                // Placeholder call - adjust to actual service method signature
                var success = await _searchService.UpdateUserPreferencesAsync(
                    userId,
                    userPrefs
                    /*
                    preferences.LastQuery ?? "", 
                    preferences.LastRelevanceScore,
                    preferences.FavoriteTags?.ToArray() ?? Array.Empty<string>()
                    */
                    );

                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating preferences for user: {UserId}", userId);
                return StatusCode(500, "An error occurred while updating user preferences");
            }
        }

        #region Helper Methods

        private SearchResultsDto MapToSearchResultsDto(string query, TrialWorld.Core.Models.Search.SearchResults results)
        {
            return new SearchResultsDto
            {
                Query = query,
                TotalHits = results.TotalMatches,
                SearchDuration = TimeSpan.FromMilliseconds(results.ElapsedMilliseconds),
                Results = results.Items?.Select(item => new SearchResultItemDto
                {
                    Id = item.Id,
                    Title = item.Title,
                    Snippet = item.TextMatches?.FirstOrDefault()?.HighlightedText ?? string.Empty,
                    ContentType = item.MediaType ?? "unknown",
                    Url = $"/api/media/{item.Id}",
                    Score = item.Score,
                    Metadata = new Dictionary<string, string?>
                    {
                        { "duration", item.DurationInSeconds.ToString() },
                        { "mediaPath", item.MediaPath },
                        { "fileDate", item.FileDate.ToString("o") },
                        { "speakers", item.Speakers != null ? string.Join(", ", item.Speakers.Select(s=>s.Label)) : null }
                    }
                }).ToList() ?? new List<SearchResultItemDto>()
            };
        }

        private TrialWorld.Core.Models.UserPreferences MapToUserPreferences(UserPreferencesDto dto)
        {
            return new TrialWorld.Core.Models.UserPreferences
            {
                FavoriteTags = dto.FavoriteTags ?? new List<string>(),
                EmotionWeights = dto.EmotionWeights ?? new Dictionary<string, double>()
                // Map other fields if they exist on the Core model
            };
        }

        #endregion
    }

    public class SearchResultsDto
    {
        public List<SearchResultItemDto> Results { get; set; } = new();
        public int TotalHits { get; set; }
        public TimeSpan SearchDuration { get; set; }
        public string Query { get; set; } = string.Empty;
    }

    public class SearchResultItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string MediaPath { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public double Score { get; set; }
        public string Snippet { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public Dictionary<string, string?> Metadata { get; set; } = new();
    }
}
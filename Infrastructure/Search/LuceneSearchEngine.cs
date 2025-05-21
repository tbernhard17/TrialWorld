using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Search;
using TrialWorld.Core.Models.Transcription;
using CoreUserPreferences = TrialWorld.Core.Models.UserPreferences;
using CoreMediaMetadata = TrialWorld.Core.Models.MediaMetadata;
using CoreSearchQuery = TrialWorld.Core.Models.Search.SearchQuery;
using SearchableContent = TrialWorld.Core.Models.SearchableContent;
using TrialWorld.Core.StreamInfo;
using System.Globalization; // Add for CultureInfo

namespace TrialWorld.Infrastructure.Search
{
    /// <summary>
    /// Implementation of the search engine using Lucene.NET
    /// </summary>
    public class LuceneSearchEngine : ISearchService
    {
        /// <summary>
        /// Event that is raised when the processing status changes.
        /// </summary>
        public event EventHandler<SearchProcessingStatus>? ProcessingStatusChanged;
        // Constants for Lucene fields (ensure these match LuceneSearchIndexer)
        private const string IdField = "Id";
        private const string FilePathField = "FilePath";
        private const string FileNameField = "FileName";
        private const string TitleField = "Title";
        private const string DescriptionField = "Description";
        private const string TranscriptField = "Transcript";
        private const string TopicsField = "Topics";
        private const string KeywordsField = "Keywords";
        private const string SpeakersField = "Speakers";
        private const string CreatedAtField = "CreatedAt";
        private const string DurationField = "Duration";
        private const string MediaTypeField = "MediaType";
        private static readonly string[] FullTextFields = { TitleField, TranscriptField, TopicsField, KeywordsField, DescriptionField, FileNameField };

        private readonly ISearchIndexer _searchIndexer;
        private readonly ILogger<LuceneSearchEngine> _logger;
        private readonly SearchOptions _options;
        private readonly StandardAnalyzer _analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);

        /// <summary>
        /// Initializes a new instance of the <see cref="LuceneSearchEngine"/> class.
        /// </summary>
        /// <param name="searchIndexer">The search indexer</param>
        /// <param name="options">The search options</param>
        /// <param name="logger">The logger</param>
        public LuceneSearchEngine(
            ISearchIndexer searchIndexer,
            IOptions<SearchOptions> options,
            ILogger<LuceneSearchEngine> logger)
        {
            _searchIndexer = searchIndexer ?? throw new ArgumentNullException(nameof(searchIndexer));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Subscribe to the indexer's processing status changed event
            if (_searchIndexer is ISearchIndexer indexer)
            {
                indexer.ProcessingStatusChanged += OnIndexerProcessingStatusChanged;
            }
        }
        
        /// <summary>
        /// Gets the current processing status of the search database.
        /// </summary>
        /// <returns>The current processing status.</returns>
        public async Task<SearchProcessingStatus> GetProcessingStatusAsync()
        {
            return await _searchIndexer.GetProcessingStatusAsync();
        }
        
        /// <summary>
        /// Handler for the indexer's processing status changed event.
        /// </summary>
        private void OnIndexerProcessingStatusChanged(object? sender, SearchProcessingStatus status)
        {
            // Forward the event to subscribers of this service
            ProcessingStatusChanged?.Invoke(this, status);
            
            // Log status changes
            if (status.IsProcessing)
            {
                _logger.LogInformation(
                    "Search database processing: {Operation} - {Progress:F1}% complete, {Processed}/{Total} files",
                    status.CurrentOperation,
                    status.ProgressPercentage,
                    status.FilesProcessed,
                    status.TotalFilesToProcess);
                
                if (!string.IsNullOrEmpty(status.CurrentFileName))
                {
                    _logger.LogDebug("Processing file: {FileName}", status.CurrentFileName);
                }
            }
            else if (status.ErrorMessages.Count > 0)
            {
                _logger.LogWarning(
                    "Search database processing completed with errors: {ErrorCount} errors occurred",
                    status.ErrorMessages.Count);
                
                foreach (var error in status.ErrorMessages)
                {
                    _logger.LogWarning("Search processing error: {Error}", error);
                }
            }
            else
            {
                _logger.LogInformation(
                    "Search database processing completed: {Operation}",
                    status.CurrentOperation);
            }
        }

        /// <inheritdoc />
        public async Task<SearchResults> SearchAsync(CoreSearchQuery query, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Executing structured search for Text: {QueryText}", query.Text);
            var stopwatch = Stopwatch.StartNew();
            try
            {
                // Build query (pass filters from the CoreSearchQuery object)
                Query luceneQuery = BuildLuceneQuery(query.Text, ConvertFilterDictionaryToSearchFilters(query.Filters)); 

                // Fetch results based on pagination
                int numToFetch = query.PageNumber * query.PageSize; // Fetch up to the required page end
                int skip = (query.PageNumber - 1) * query.PageSize;

                var rawResults = await _searchIndexer.SearchAsync(luceneQuery, numToFetch, 0, cancellationToken); // SearchAsync handles fetching

                // Map results directly without ranking
                var mappedItems = rawResults
                    .Skip(skip) // Apply pagination here after fetching potentially more
                    .Take(query.PageSize)
                    .Select(r => MapContentToResultItem(r, query.Text))
                    .ToList();

                // TODO: Implement facet generation based on rawResults if needed
                // TODO: Implement related query generation if needed


                var searchResults = new SearchResults
                {
                    Items = mappedItems,
                    TotalMatches = rawResults.Count, // This is total fetched, might need refinement for true total matches
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
                };

                stopwatch.Stop();
                _logger.LogInformation("Structured search completed in {ElapsedMs}ms finding {MatchCount} matches.", stopwatch.ElapsedMilliseconds, searchResults.TotalMatches);
                return searchResults;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during structured search for query: {QueryText}", query.Text);
                throw; // Rethrow to allow higher layers to handle
            }
        }

        /// <inheritdoc />
        public Task<SearchResults> SemanticSearchAsync(
            string concept,
            SearchFilters? filters = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            // Basic implementation: Treat concept as a regular text query
            // Semantic expansion logic removed
            _logger.LogInformation("Performing basic text search for concept: {Concept}", concept);

            // Convert SearchFilters to dictionary for CoreSearchQuery
             var filterDict = CreateFilterDictionary(filters);

            var query = new CoreSearchQuery {
                Text = concept, // Use concept directly as search text
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filters = filterDict,
                SortBy = "" // Default sort (likely by relevance)
            };

            return SearchAsync(query, cancellationToken);
        }

        public async Task<SearchResults> SearchBySpeakerAsync(string speakerName, string? queryText = null, SearchFilters? filters = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(speakerName))
                throw new ArgumentException("Speaker name cannot be empty.", nameof(speakerName));

            _logger.LogInformation("Searching by Speaker: {Speaker}, Query: \"{QueryText}\"", speakerName, queryText);

            // Ensure filters object exists if null
            filters ??= new SearchFilters();
            
            // Add speaker to the filters hashset
             filters.Speakers ??= new HashSet<string>();
             filters.Speakers.Add(speakerName);


            // Convert combined filters to dictionary
             var filterDict = CreateFilterDictionary(filters);


            var querySpeaker = new CoreSearchQuery {
                Text = queryText ?? "*", // Search for queryText or everything if null
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filters = filterDict,
                SortBy = "" 
            };

            return await SearchAsync(querySpeaker, cancellationToken);
        }

        public async Task<bool> IndexMediaAsync(
            string mediaId,
            CoreMediaMetadata mediaMetadata,
            TranscriptionResult? transcription = null,
            ContentAnalysisResult? contentAnalysis = null,
            CancellationToken cancellationToken = default)
        {
             _logger.LogInformation("Indexing media: {MediaId}", mediaId);
             
             // Log detailed information about the media being indexed
             _logger.LogDebug("Media details - Title: {Title}, Duration: {Duration}, Type: {MediaType}",
                 mediaMetadata.Title,
                 mediaMetadata.Duration,
                 mediaMetadata.MediaType);
             
             if (transcription != null)
             {
                 _logger.LogDebug("Transcription available - Length: {Length} chars, Segments: {SegmentCount}",
                     transcription.Transcript?.Length ?? 0,
                     transcription.Segments?.Count ?? 0);
             }
             
             if (contentAnalysis != null)
             {
                 _logger.LogDebug("Content analysis available - Topics: {TopicCount}, Highlights: {HighlightCount}",
                     contentAnalysis.Topics?.Count ?? 0,
                     contentAnalysis.Highlights?.Count ?? 0);
             }
             
             // Simplified: Convert to SearchableContent and call indexer
             // Note: Face, emotion, gesture data from parameters is now ignored
             var content = new SearchableContent
             {
                 Id = mediaId,
                 MediaId = mediaId, // Assuming MediaId is the primary identifier
                 Title = mediaMetadata.Title,
                 Description = mediaMetadata.Metadata?.GetValueOrDefault("description", string.Empty) ?? string.Empty,
                 Transcript = transcription?.Transcript ?? string.Empty,
                 // Speakers moved to Metadata
                 // Keywords moved to Metadata
                 Topics = mediaMetadata.Metadata?.GetValueOrDefault("topics", "")?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? new List<string>(),
                 CreatedAt = mediaMetadata.CreatedDate,
                 LastModified = mediaMetadata.ModifiedDate,
                 Duration = mediaMetadata.Duration,
                 // MediaType moved to Metadata
                 ThumbnailUrl = mediaMetadata.ThumbnailUrl, // Keep thumbnail URL if generated elsewhere
                 // Populate Snippets if transcription available
                 Snippets = transcription?.Segments?.Select(s => new SearchSnippet {
                     Text = s.Text,
                     Start = TimeSpan.FromMilliseconds(s.StartTime),
                     End = TimeSpan.FromMilliseconds(s.EndTime),
                     Confidence = (float)s.Confidence, // Explicit cast from double to float
                     Speaker = s.Speaker ?? string.Empty
                     // Sentiment and Words from s are not directly mapped to SearchSnippet here
                     }).ToList() ?? new List<SearchSnippet>(),
                 // Face, Emotion, Gesture data not added here
                 Metadata = new Dictionary<string, string>() // Initialize Metadata
             };

             // Populate Metadata from available sources
             content.Metadata["Speakers"] = string.Join(",", transcription?.Speakers?.Select(s => s.Name ?? $"Speaker {s.Id}").Distinct() ?? Enumerable.Empty<string>());
             content.Metadata["Keywords"] = mediaMetadata.Metadata?.GetValueOrDefault("keywords", "") ?? string.Empty;
             content.Metadata["MediaType"] = mediaMetadata.MediaType.ToString();
             content.Metadata["Description"] = content.Description; // Add description to metadata too if needed for indexing/filtering

             // Call the indexer and await the result
             bool success = await _searchIndexer.IndexAsync(mediaId, content, cancellationToken);

             if (success)
             {
                 _logger.LogInformation("Successfully indexed media: {MediaId}", mediaId);
             }
             else
             {
                 _logger.LogWarning("Failed to index media: {MediaId}", mediaId);
             }

             return success; // Return the bool result
        }

        public Task<bool> UpdateMediaAsync(
            string mediaId,
            CoreMediaMetadata mediaMetadata,
            CancellationToken cancellationToken = default)
        {
             _logger.LogInformation("Updating media in index: {MediaId}", mediaId);
             // Fetch existing associated data if needed (e.g., transcription) to rebuild SearchableContent
             // This is simplified - assumes we re-index with just the provided metadata
             // A full implementation might need to merge existing indexed data (like transcript)
             var content = new SearchableContent
             {
                 Id = mediaId,
                 MediaId = mediaId,
                 Title = mediaMetadata.Title,
                 Description = mediaMetadata.Metadata?.GetValueOrDefault("description", string.Empty) ?? string.Empty,
                 CreatedAt = mediaMetadata.CreatedDate,
                 LastModified = mediaMetadata.ModifiedDate,
                 Duration = mediaMetadata.Duration,
                 // MediaType moved to Metadata
                 ThumbnailUrl = mediaMetadata.ThumbnailUrl,
                 // Ideally, fetch existing transcript/speakers/topics etc. and merge
                 // For now, potentially overwrites existing data if not included in CoreMediaMetadata
                 Transcript = string.Empty, // Placeholder - should fetch existing if possible
                 // Speakers moved to Metadata
                 Topics = mediaMetadata.Metadata?.GetValueOrDefault("topics", "")?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? new List<string>(),
                 Snippets = new List<SearchSnippet>(), // Placeholder
                 Metadata = new Dictionary<string, string>() // Initialize Metadata
             };

             // Populate Metadata from available sources
             content.Metadata["Keywords"] = mediaMetadata.Metadata?.GetValueOrDefault("keywords", "") ?? string.Empty;
             content.Metadata["MediaType"] = mediaMetadata.MediaType.ToString();
             content.Metadata["Speakers"] = string.Empty; // Placeholder - should fetch existing if possible
             content.Metadata["Description"] = content.Description;

            return _searchIndexer.UpdateAsync(mediaId, content, cancellationToken);
        }

        public Task<bool> RemoveFromIndexAsync(string mediaId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Removing media from index: {MediaId}", mediaId);
            return _searchIndexer.DeleteAsync(mediaId, cancellationToken);
        }

        public async Task<bool> ClearIndexAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Clearing entire search index.");
            
            // Get current statistics before clearing
            var stats = await _searchIndexer.GetStatisticsAsync();
            _logger.LogInformation("Clearing index with {DocumentCount} documents", stats.TotalDocuments);
            
            var result = await _searchIndexer.ClearAllAsync(cancellationToken);
            
            if (result)
            {
                _logger.LogInformation("Successfully cleared search index");
            }
            else
            {
                _logger.LogError("Failed to clear search index");
            }
            
            return result;
        }

        public Task<IndexStatistics> GetIndexStatisticsAsync()
        {
             _logger.LogInformation("Getting index statistics.");
             return _searchIndexer.GetStatisticsAsync();
        }

        public Task<IList<string>> GetQuerySuggestionsAsync(string partialQuery, int maxSuggestions = 5, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting query suggestions for: {PartialQuery}", partialQuery);
            // Basic placeholder implementation
             var suggestions = new List<string>();
             if (!string.IsNullOrWhiteSpace(partialQuery))
             {
                 suggestions.Add(partialQuery + " suggestion 1");
                 suggestions.Add(partialQuery + " suggestion 2");
             }
            return Task.FromResult<IList<string>>(suggestions.Take(maxSuggestions).ToList());
        }

        public Task<SearchResults> AdvancedSearchAsync(AdvancedSearchCriteria criteria, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Performing advanced search.");

            // Convert AdvancedSearchCriteria filters to dictionary
             var filtersDictAdvanced = CreateFilterDictionary(criteria.Filters);

            var queryAdvanced = new CoreSearchQuery {
                Text = criteria.Query ?? "*", // Use provided query or search all
                PageNumber = criteria.Skip / criteria.MaxResults + 1, // Calculate page number from skip/max
                PageSize = criteria.MaxResults,
                Filters = filtersDictAdvanced,
                SortBy = criteria.SortField ?? "" // Use provided sort field or default
            };

            // Execute the standard search with the constructed query object
            return SearchAsync(queryAdvanced, cancellationToken);
        }

        public Task<bool> UpdateUserPreferencesAsync(string userId, CoreUserPreferences preferences, CancellationToken cancellationToken = default)
        {
            // AI Ranker removed, so preferences might not be used for ranking anymore.
            // Can still store them if needed for other purposes.
            _logger.LogWarning("UpdateUserPreferencesAsync called, but AI ranking based on preferences is disabled. Storing preferences is not implemented.");
            return Task.FromResult(true); // Indicate success without doing anything
        }

        private Query BuildLuceneQuery(string queryText, SearchFilters? filters)
        {
            var topLevelQuery = new BooleanQuery();

            // Text Query Part
            Query? textQueryPart = BuildTextQuery(queryText);
            if (textQueryPart != null)
            {
                // Use MUST: results must match the text query
                topLevelQuery.Add(textQueryPart, Occur.MUST);
            }
            else if (filters == null || !filters.HasAnyFilter)
            {
                // If no text query AND no filters, match everything
                return new MatchAllDocsQuery();
            }
            else
            {
                 // If filters exist but no text query, add a placeholder MUST clause
                 // that doesn't restrict results, allowing filters to apply.
                 topLevelQuery.Add(new MatchAllDocsQuery(), Occur.MUST);
            }

            // Filter Query Part
            if (filters != null && filters.HasAnyFilter)
            {
                var filterQuery = new BooleanQuery(); // Use BooleanQuery for filters as well

                // Add clauses for each active filter
                AddFilterClause(filterQuery, SpeakersField, filters.Speakers);
                AddFilterClause(filterQuery, KeywordsField, filters.Keywords);
                AddFilterClause(filterQuery, TopicsField, filters.Topics);
                AddDateRangeFilter(filterQuery, filters.DateRange);
                AddDurationFilter(filterQuery, filters.DurationRange);

                // Add the combined filter query part as a MUST clause
                 if (filterQuery.Clauses.Any())
                 {
                     // Use FILTER (all caps) instead of Filter
                    
                 }
            }

             // If the query ended up empty (e.g., only filters that didn't create clauses), return MatchAll
             if (!topLevelQuery.Clauses.Any())
             {
                 return new MatchAllDocsQuery();
             }

            return topLevelQuery;
        }

        private Query? BuildTextQuery(string queryText)
        {
            if (string.IsNullOrWhiteSpace(queryText) || queryText == "*") return null;
            // Use MultiFieldQueryParser to search across specified text fields
            var parser = new MultiFieldQueryParser(LuceneVersion.LUCENE_48, FullTextFields, _analyzer)
            {
                DefaultOperator = Operator.OR // Default to OR for broader results unless specified otherwise
            };
            try
            {
                // Parse the query text
                return parser.Parse(queryText);
            }
            catch (ParseException ex)
            {
                _logger.LogError(ex, "Failed to parse query text: {QueryText}. Falling back to TermQuery.", queryText);
                // Fallback to a simple term query on the Title field if parsing fails
                return new TermQuery(new Term(TitleField, queryText.ToLowerInvariant()));
            }
        }

        private string ExpandConceptToKeywords(string concept)
        {
            // Placeholder - Semantic expansion logic removed
            _logger.LogDebug("Semantic concept expansion is disabled. Using concept directly: '{Concept}'", concept);
            return concept; // Return the concept itself as keywords
        }

        private SearchResultItem MapContentToResultItem(SearchableContent content, string query)
        {
            if (content == null)
            {
                _logger.LogWarning("Attempted to map null SearchableContent.");
                // Return a default or throw? Returning default might hide issues.
                throw new ArgumentNullException(nameof(content), "Cannot map null SearchableContent to SearchResultItem.");
            }

             // Basic mapping from SearchableContent (Lucene doc representation) to SearchResultItem (API/UI model)
             // Note: Highlighting/snippet generation would typically happen here based on the query
             // This is a simplified mapping.

            var resultItem = new SearchResultItem
            {
                Id = content.Id ?? content.MediaId ?? Guid.NewGuid().ToString(), // Ensure an ID exists
                MediaPath = content.FilePath, // Use FilePath (read-only property)
                Title = content.Title ?? content.FileName ?? "Untitled", // Use Title or FileName
                Score = content.Score, // Score assigned by Lucene during search
                MediaType = content.Metadata?.GetValueOrDefault(MediaTypeField, "Unknown") ?? "Unknown", // Get from Metadata using constant
                DurationInSeconds = content.Duration.TotalSeconds,
                FileDate = content.LastModified, // Or CreatedAt? Depends on desired semantics
                FileSizeBytes = 0, // SearchableContent doesn't seem to store this directly

                 // Map Text Matches (simple approach: add full transcript as a single match)
                 TextMatches = new List<TextMatch>(), // Initialize

                MatchTimestamps = content.Timestamps?.Select(t => new MatchTimestamp {
                     TimeInSeconds = t.Start.TotalSeconds, // Use Start.TotalSeconds
                     Confidence = t.Confidence, // Confidence is float, MatchTimestamp expects double
                     MatchType = "timestamp" // Removed Metadata access
                     // ThumbnailPath removed
                 }).ToList() ?? new List<MatchTimestamp>(),


                // Map Speakers (get from Metadata dictionary using constant)
                 Speakers = content.Metadata?.GetValueOrDefault(SpeakersField, "")?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Select(s => new SpeakerSummary { Label = s }).ToList() ?? new List<SpeakerSummary>(),

                 // Map Topics (directly if available)
                 Topics = content.Topics ?? new List<string>()

                 // Removed Faces mapping
                 // Removed Emotions mapping
                 // Removed Gestures mapping
                 // Removed ThumbnailPaths mapping
            };
            
            // Populate TextMatches with snippets if available
            if (content.Snippets != null && content.Snippets.Any())
            {
                resultItem.TextMatches = content.Snippets.Select(s => new TextMatch {
                    HighlightedText = s.Text ?? string.Empty, // Basic text, no highlighting applied here
                    StartTime = s.Start.TotalSeconds, // Use Start from base class SearchableTimestamp
                    EndTime = s.End.TotalSeconds, // Use End from base class SearchableTimestamp
                    Speaker = s.Speaker // Map speaker if available on snippet
                }).ToList();
            }
            else if (!string.IsNullOrEmpty(content.Transcript))
            {
                // Fallback: Add the entire transcript as one text match if no snippets
                 resultItem.TextMatches.Add(new TextMatch { HighlightedText = content.Transcript, StartTime = 0, EndTime = content.Duration.TotalSeconds });
            }

            return resultItem;
        }

        private void AddFilterClause(BooleanQuery parentFilterQuery, string fieldName, IEnumerable<string>? values)
        {
            if (values == null || !values.Any()) return;

            var fieldQuery = new BooleanQuery();
            // Use SHOULD: document must match at least one of the values for this field
            foreach (var value in values.Where(v => !string.IsNullOrWhiteSpace(v)))
            {
                // Use TermQuery for exact matches (assuming standard analysis for these fields)
                fieldQuery.Add(new TermQuery(new Term(fieldName, value.ToLowerInvariant())), Occur.SHOULD);
            }

            if (fieldQuery.Clauses.Count > 0)
            {
                 // Add this field's criteria as MUST to the parent filter query
                 // Meaning the document MUST satisfy the conditions for this field (match at least one value)
                 parentFilterQuery.Add(fieldQuery, Occur.MUST);
            }
        }

        private void AddDateRangeFilter(BooleanQuery filterQuery, TrialWorld.Core.Models.Search.DateTimeRange? range)
        {
            if (range == null || (!range.Start.HasValue && !range.End.HasValue)) return;

            // Assuming CreatedAtField is indexed numerically (e.g., Ticks or epoch seconds) for efficient range queries
            // If indexed as string (e.g., "yyyyMMddHHmmss"), use TermRangeQuery instead.
            // Let's assume Ticks for this example.
            long? startTicks = range.Start?.ToUniversalTime().Ticks;
            long? endTicks = range.End?.ToUniversalTime().Ticks;

            if (startTicks.HasValue || endTicks.HasValue)
            {
                filterQuery.Add(NumericRangeQuery.NewInt64Range(CreatedAtField,
                                                               startTicks,
                                                               endTicks,
                                                               true, // include lower bound
                                                               true), // include upper bound
                                Occur.MUST); // Add as MUST clause to the filter query
            }
        }

        private void AddDurationFilter(BooleanQuery filterQuery, TrialWorld.Core.Models.Search.TimeSpanRange? range)
        {
            if (range == null || (!range.Start.HasValue && !range.End.HasValue)) return;

            // Assuming DurationField is indexed numerically (TotalSeconds as double)
            double? minSeconds = range.Start?.TotalSeconds;
            double? maxSeconds = range.End?.TotalSeconds;

            AddNumericRangeFilter(filterQuery, DurationField, minSeconds, maxSeconds);
        }

        private void AddNumericRangeFilter(BooleanQuery filterQuery, string fieldName, double? minInclusive, double? maxInclusive)
        {
            if (minInclusive.HasValue || maxInclusive.HasValue)
            {
                filterQuery.Add(NumericRangeQuery.NewDoubleRange(fieldName,
                                                               minInclusive,
                                                               maxInclusive,
                                                               true, // minInclusive flag
                                                               true), // maxInclusive flag
                                Occur.MUST); // Add as MUST clause to the filter query
            }
        }

        // Facet generation might need simplification if AI fields were used for facets

        // Related query generation might depend on AI/semantics, simplify if needed

        // Helper to convert Dictionary to SearchFilters (simplified version)
        private SearchFilters? ConvertFilterDictionaryToSearchFilters(Dictionary<string, string> filterDict)
        {
            if (filterDict == null || filterDict.Count == 0) return null;

            var filters = new SearchFilters();
             if (filterDict.TryGetValue("date_start", out var dateStartStr) && DateTime.TryParse(dateStartStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateStart))
                 filters.DateRange = new DateTimeRange { Start = dateStart, End = filters.DateRange?.End };
            if (filterDict.TryGetValue("date_end", out var dateEndStr) && DateTime.TryParse(dateEndStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateEnd))
                 filters.DateRange = new DateTimeRange { Start = filters.DateRange?.Start, End = dateEnd };
            
            if (filterDict.TryGetValue("duration_min_seconds", out var minDurationStr) && double.TryParse(minDurationStr, CultureInfo.InvariantCulture, out var minDuration))
                filters.DurationRange = new TimeSpanRange { Start = TimeSpan.FromSeconds(minDuration), End = filters.DurationRange?.End };
            if (filterDict.TryGetValue("duration_max_seconds", out var maxDurationStr) && double.TryParse(maxDurationStr, CultureInfo.InvariantCulture, out var maxDuration))
                 filters.DurationRange = new TimeSpanRange { Start = filters.DurationRange?.Start, End = TimeSpan.FromSeconds(maxDuration) };

            if (filterDict.TryGetValue("media_types", out var mediaTypesStr))
                filters.MediaTypes = new HashSet<string>(mediaTypesStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
             if (filterDict.TryGetValue("speakers", out var speakersStr))
                 filters.Speakers = new HashSet<string>(speakersStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
             if (filterDict.TryGetValue("topics", out var topicsStr))
                 filters.Topics = new HashSet<string>(topicsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
             if (filterDict.TryGetValue("keywords", out var keywordsStr))
                 filters.Keywords = new HashSet<string>(keywordsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            
             if (filterDict.TryGetValue("minConfidence", out var minConfidenceStr) && double.TryParse(minConfidenceStr, CultureInfo.InvariantCulture, out var minConfidence))
                 filters.MinConfidence = minConfidence;

            return filters.HasAnyFilter ? filters : null;
        }

        private static Dictionary<string, string> CreateFilterDictionary(SearchFilters? filters)
        {
            var filterDict = new Dictionary<string, string>();
            if (filters == null) return filterDict;

            // Date Range
            if (filters.DateRange != null)
            {
                if (filters.DateRange.Start.HasValue)
                    filterDict["date_start"] = filters.DateRange.Start.Value.ToString("o", CultureInfo.InvariantCulture); // ISO 8601
                if (filters.DateRange.End.HasValue)
                    filterDict["date_end"] = filters.DateRange.End.Value.ToString("o", CultureInfo.InvariantCulture);
            }

            // Duration Range
            if (filters.DurationRange != null)
            {
                 // Use Start/End properties of TimeSpanRange
                 if (filters.DurationRange.Start.HasValue) 
                     filterDict["duration_min_seconds"] = filters.DurationRange.Start.Value.TotalSeconds.ToString("F3", CultureInfo.InvariantCulture);
                 if (filters.DurationRange.End.HasValue) 
                     filterDict["duration_max_seconds"] = filters.DurationRange.End.Value.TotalSeconds.ToString("F3", CultureInfo.InvariantCulture);
            }

            // HashSets -> Comma-separated strings
            if (filters.MediaTypes?.Count > 0)
                filterDict["media_types"] = string.Join(",", filters.MediaTypes);

            if (filters.Speakers?.Count > 0)
                filterDict["speakers"] = string.Join(",", filters.Speakers);

            if (filters.Topics?.Count > 0)
                filterDict["topics"] = string.Join(",", filters.Topics);

            if (filters.Keywords?.Count > 0)
                filterDict["keywords"] = string.Join(",", filters.Keywords);

            // Other scalar filters
            if (filters.MinConfidence.HasValue)
                filterDict["minConfidence"] = filters.MinConfidence.Value.ToString(CultureInfo.InvariantCulture);

            return filterDict;
        }

        // Add missing BuildIndexAsync method from ISearchService (placeholder implementation)
        public Task<IndexingResult> BuildIndexAsync(IList<string> mediaDirectories, bool incremental = true, IProgress<IndexingProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("BuildIndexAsync is not fully implemented in LuceneSearchEngine.");
             // Placeholder implementation
             var result = new IndexingResult { // Initialize required fields
                 Success = false,
                 ProcessedFiles = 0,
                 FailedFiles = 0,
                 FailedFilePaths = new List<string>(), // Initialize required list
                 ErrorMessages = new Dictionary<string, string>() // Initialize required dictionary
                 };
             // Correctly report progress without setting read-only properties
             progress?.Report(new IndexingProgress {
                 TotalFiles = 0, // Placeholder value
                 ProcessedFiles = 0,
                 FailedFiles = 0,
                 CurrentOperation = "BuildIndexAsync Not Implemented"
                 // IsComplete is calculated automatically
                 });
             return Task.FromResult(result);
        }

        // Add missing IndexContentAsync implementation
        public async Task<bool> IndexContentAsync(
            string mediaId,
            string content,
            TimeSpan duration,
            IEnumerable<SearchableTimestamp> timestamps,
            IEnumerable<ContentTopic> topics,
            IEnumerable<ContentSegment> segments, // Note: ContentSegment lacks SpeakerId
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Indexing content for media ID: {MediaId}", mediaId);
            // Convert parameters into a SearchableContent object for the indexer
            var searchable = new SearchableContent
            {
                Id = mediaId,
                MediaId = mediaId,
                Transcript = content,
                Duration = duration,
                Timestamps = timestamps?.ToList() ?? new List<SearchableTimestamp>(),
                Topics = topics?.Select(t => t.Name ?? string.Empty).ToList() ?? new List<string>(),
                Snippets = segments?.Select(s => new SearchSnippet {
                    Start = s.StartTime, // Map from ContentSegment.StartTime
                    End = s.EndTime,     // Map from ContentSegment.EndTime
                    Text = string.Empty, // ContentSegment doesn't have Text? Use empty or derive
                    Confidence = (float)s.Confidence, // Map confidence
                    Speaker = string.Empty // ContentSegment doesn't have SpeakerId, set empty
                    }).ToList() ?? new List<SearchSnippet>(),
                 Metadata = new Dictionary<string, string>() // Initialize metadata
                 // TODO: Add relevant data like speakers, keywords, media type to Metadata
                 // Example: Metadata["Keywords"] = string.Join(",", topics.Select(t => t.Name));
            };

            // Populate basic metadata if possible
            searchable.Metadata["Keywords"] = string.Join(",", searchable.Topics);
            // Speaker info not directly available from ContentSegment list


            return await _searchIndexer.IndexAsync(mediaId, searchable, cancellationToken); // Use await
        }
    }
} 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models.Search;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Transcription;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.LuceneVersion;

namespace TrialWorld.Infrastructure.Search
{
    /// <summary>
    /// Implements the search indexer using Lucene.NET
    /// </summary>
    public class LuceneSearchIndexer : ISearchIndexer, IDisposable
    {
        /// <summary>
        /// Event that is raised when the processing status changes.
        /// </summary>
        public event EventHandler<Core.Models.Search.SearchProcessingStatus>? ProcessingStatusChanged;
        
        // Current processing status - using a backing field for the property
        private Core.Models.Search.SearchProcessingStatus _processingStatus = Core.Models.Search.SearchProcessingStatus.Idle;
        
        // Lock for status updates
        private readonly object _processingStatusLock = new object();
        // Constants for Lucene fields (can be defined here or passed from SearchEngine)
        private const string IdField = "Id";
        private const string FilePathField = "FilePath";
        private const string FileNameField = "FileName";
        private const string TitleField = "Title";
        private const string DescriptionField = "Description";
        private const string TranscriptField = "Transcript";
        private const string TopicsField = "Topics";
        private const string KeywordsField = "Keywords";
        private const string SpeakersField = "Speakers";
        // Removed FaceIdField, EmotionField, EmotionIntensityField, GestureField, GestureConfidenceField constants if they were defined here
        private const string CreatedAtField = "CreatedAt";
        private const string DurationField = "Duration";
        private const string MediaTypeField = "MediaType";
        // Add constants for numeric fields if not already present
        private const string CreatedAtTicksField = "CreatedAtTicks"; 
        private const string DurationSecondsField = "DurationSeconds";

        private static readonly LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

        private readonly Directory _directory;
        private readonly StandardAnalyzer _analyzer;
        private readonly IndexWriter _writer;
        private readonly ILogger<LuceneSearchIndexer> _logger;
        private readonly SearchOptions _options;
        private readonly object _writerLock = new object();
        private bool _disposed = false;
        

        /// <summary>
        /// Initializes a new instance of the <see cref="LuceneSearchIndexer"/> class.
        /// </summary>
        /// <param name="options">Configuration options</param>
        /// <param name="logger">Logger</param>
        public LuceneSearchIndexer(IOptions<SearchOptions> options, ILogger<LuceneSearchIndexer> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Create directory if it doesn't exist
            string indexPath = _options.IndexDirectory ?? Path.Combine(AppContext.BaseDirectory, "search_index");
            if (!System.IO.Directory.Exists(indexPath))
            {
                System.IO.Directory.CreateDirectory(indexPath);
                _logger.LogInformation("Created search index directory at: {IndexPath}", indexPath);
            }
            else
            {
                _logger.LogInformation("Using existing search index at: {IndexPath}", indexPath);
            }

            _analyzer = new StandardAnalyzer(AppLuceneVersion);
            _directory = FSDirectory.Open(new DirectoryInfo(indexPath));

            // Check for lock file and release if necessary (handle potential stale locks)
            if (IndexWriter.IsLocked(_directory))
            {
                _logger.LogWarning("Search index directory was locked. Attempting to release lock.");
                IndexWriter.Unlock(_directory);
            }

            var config = new IndexWriterConfig(AppLuceneVersion, _analyzer)
            {
                OpenMode = OpenMode.CREATE_OR_APPEND
            };

            _writer = new IndexWriter(_directory, config);
        }

        /// <inheritdoc />
        public Task<bool> IndexAsync(string mediaId, SearchableContent content, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(mediaId) || content == null)
            {
                _logger.LogWarning("IndexAsync called with null or empty mediaId/content.");
                return Task.FromResult(false);
            }
            
            _logger.LogDebug("Indexing content for media ID: {MediaId}", mediaId);
            
            // Update processing status
            UpdateProcessingStatus(status => {
                status.IsProcessing = true;
                status.CurrentOperation = "Indexing";
                status.CurrentMediaId = mediaId;
                status.CurrentFileName = content.FileName;
                return status;
            });
            
            return Task.Run(() =>
            {
                try
                {
                    var doc = CreateDocument(mediaId, content);
                    lock (_writerLock)
                    {
                        _writer.UpdateDocument(new Term("Id", mediaId), doc);
                    }
                    
                    // Update processing status to idle
                    UpdateProcessingStatus(status => {
                        status.IsProcessing = false;
                        status.CurrentOperation = "Idle";
                        status.CurrentMediaId = null;
                        status.CurrentFileName = null;
                        return status;
                    });
                    
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error indexing content for media ID: {MediaId}", mediaId);
                    
                    // Update processing status with error
                    UpdateProcessingStatus(status => {
                        status.IsProcessing = false;
                        status.CurrentOperation = "Error";
                        status.AddError($"Failed to index media {mediaId}: {ex.Message}");
                        return status;
                    });
                    
                    return false;
                }
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<List<SearchableContent>> SearchAsync(object queryObj, int maxResults, int skipResults = 0, CancellationToken cancellationToken = default)
        {
            if (queryObj is not Query query)
            {
                _logger.LogError("Invalid query object type passed to SearchAsync. Expected Lucene.Net.Search.Query, got {QueryType}", queryObj?.GetType().FullName ?? "null");
                // Return empty list of the correct type
                return Task.FromResult(new List<SearchableContent>());
            }

            if (maxResults <= 0) maxResults = _options.DefaultResultsLimit;
            _logger.LogDebug("Searching index with Query: [{LuceneQuery}], MaxResults: {MaxResults}, Skip: {Skip}", query.ToString(), maxResults, skipResults);

            return Task.Run(async () =>
            {
                if (!DirectoryReader.IndexExists(_directory))
                {
                    _logger.LogWarning("Search index does not exist at {IndexPath}", _options.IndexDirectory);
                    // Return empty list of the correct type
                    return new List<SearchableContent>();
                }

                DirectoryReader reader = null!;
                IndexSearcher searcher = null!;
                try
                {
                    await EnsureWriterCommittedAsync(cancellationToken);
                    reader = DirectoryReader.Open(_directory);
                    searcher = new IndexSearcher(reader);

                    int numHits = skipResults + maxResults;
                    TopDocs topDocs = searcher.Search(query, numHits);

                    _logger.LogDebug("Query found {TotalHits} total hits.", topDocs.TotalHits);

                    // Change list type and call the renamed helper
                    var results = new List<SearchableContent>();
                    for (int i = skipResults; i < topDocs.ScoreDocs.Length; i++)
                    {
                        ScoreDoc scoreDoc = topDocs.ScoreDocs[i];
                        Document doc = searcher.Doc(scoreDoc.Doc);
                        // Call the renamed helper and assign score
                        var content = DocumentToSearchableContent(doc);
                        content.Score = scoreDoc.Score; // Assign score after creation
                        results.Add(content);
                    }
                    return results;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error searching index with Query: [{LuceneQuery}]", query.ToString());
                    // Return empty list of the correct type
                    return new List<SearchableContent>();
                }
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<bool> UpdateAsync(string mediaId, SearchableContent content, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("UpdateAsync called for media ID: {MediaId}. Will perform add/replace via IndexAsync.", mediaId);
            return IndexAsync(mediaId, content, cancellationToken);
        }

        /// <inheritdoc />
        public Task<bool> DeleteAsync(string mediaId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(mediaId))
            {
                _logger.LogWarning("DeleteAsync called with null or empty mediaId.");
                return Task.FromResult(false);
            }
            _logger.LogInformation("Deleting content from index for media ID: {MediaId}", mediaId);
            return Task.Run(() =>
            {
                try
                {
                    lock (_writerLock)
                    {
                        _writer.DeleteDocuments(new Term("Id", mediaId));
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting content for media ID: {MediaId}", mediaId);
                    return false;
                }
            }, cancellationToken);
        }

        public async Task<bool> ClearAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Attempting to clear the entire search index.");
            
            // Update processing status
            UpdateProcessingStatus(status => {
                status.IsProcessing = true;
                status.CurrentOperation = "Clearing Index";
                status.ProgressPercentage = 0;
                status.OperationStartedAt = DateTime.UtcNow;
                return status;
            });
            
            try
            {
                // Use Task.Run for potentially long-running synchronous Lucene operation
                await Task.Run(() =>
                {
                    lock (_writerLock)
                    {
                        _writer.DeleteAll(); // Deletes all documents
                        _writer.Commit(); // Commit the deletion
                    }
                }, cancellationToken);
                
                _logger.LogInformation("Successfully cleared the search index.");
                
                // Update processing status to idle
                UpdateProcessingStatus(status => {
                    status.IsProcessing = false;
                    status.CurrentOperation = "Idle";
                    status.ProgressPercentage = 100;
                    return status;
                });
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing the search index.");
                
                // Update processing status with error
                UpdateProcessingStatus(status => {
                    status.IsProcessing = false;
                    status.CurrentOperation = "Error";
                    status.AddError($"Failed to clear index: {ex.Message}");
                    return status;
                });
                
                return false;
            }
        }

        public async Task<IndexStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting index statistics.");
            try
            {
                // Statistics require reading from the index, so ensure writes are committed
                await EnsureWriterCommittedAsync(cancellationToken);

                // Use Task.Run as reader operations might involve I/O
                return await Task.Run(() =>
                {
                    if (!DirectoryReader.IndexExists(_directory))
                    {
                        _logger.LogWarning("Cannot get statistics: Search index does not exist.");
                        return new IndexStatistics { IndexStatus = "NonExistent" };
                    }

                    DirectoryReader reader = null!;
                    try
                    {
                        reader = DirectoryReader.Open(_directory);
                        // Get current processing status
                        Core.Models.Search.SearchProcessingStatus status;
                        lock (_processingStatusLock)
                        {
                            status = _processingStatus;
                        }
                        
                        var stats = new IndexStatistics
                        {
                            TotalDocuments = reader.NumDocs, // Total number of documents
                            // TODO: Calculate TotalSizeBytes - Requires iterating segment info or estimating
                            // TODO: Calculate LastModified - Requires getting commit user data or file timestamps
                            IndexStatus = status.IsProcessing ? "Processing" : "Available",
                            IsProcessing = status.IsProcessing,
                            CurrentOperation = status.CurrentOperation,
                            CurrentMediaId = status.CurrentMediaId,
                            CurrentFileName = status.CurrentFileName,
                            ProgressPercentage = status.ProgressPercentage,
                            FilesProcessed = status.FilesProcessed,
                            TotalFilesToProcess = status.TotalFilesToProcess,
                            EstimatedTimeRemaining = status.EstimatedTimeRemaining
                        };
                        
                        // Add processing status info to additional stats
                        if (status.IsProcessing)
                        {
                            stats.AdditionalStats["CurrentOperation"] = status.CurrentOperation;
                            stats.AdditionalStats["ProgressPercentage"] = status.ProgressPercentage.ToString("F2");
                            if (!string.IsNullOrEmpty(status.CurrentMediaId))
                            {
                                stats.AdditionalStats["CurrentMediaId"] = status.CurrentMediaId;
                            }
                            if (!string.IsNullOrEmpty(status.CurrentFileName))
                            {
                                stats.AdditionalStats["CurrentFileName"] = status.CurrentFileName;
                            }
                        }
                        
                        return stats;
                    }
                    finally
                    {
                        reader?.Dispose();
                    }
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting index statistics.");
                return new IndexStatistics { IndexStatus = "Error" };
            }
        }
        
        /// <summary>
        /// Gets the current processing status of the search indexer.
        /// </summary>
        /// <returns>The current processing status.</returns>
        public Task<Core.Models.Search.SearchProcessingStatus> GetProcessingStatusAsync()
        {
            lock (_processingStatusLock)
            {
                // Return a copy of the current status to avoid external modification
                return Task.FromResult(new Core.Models.Search.SearchProcessingStatus
                {
                    IsProcessing = _processingStatus.IsProcessing,
                    CurrentOperation = _processingStatus.CurrentOperation,
                    CurrentMediaId = _processingStatus.CurrentMediaId,
                    CurrentFileName = _processingStatus.CurrentFileName,
                    OperationStartedAt = _processingStatus.OperationStartedAt,
                    ProgressPercentage = _processingStatus.ProgressPercentage,
                    FilesProcessed = _processingStatus.FilesProcessed,
                    TotalFilesToProcess = _processingStatus.TotalFilesToProcess,
                    EstimatedTimeRemaining = _processingStatus.EstimatedTimeRemaining,
                    ErrorMessages = new List<string>(_processingStatus.ErrorMessages),
                    AdditionalInfo = new Dictionary<string, string>(_processingStatus.AdditionalInfo)
                });
            }
        }
        
        /// <summary>
        /// Updates the current processing status and raises the ProcessingStatusChanged event.
        /// </summary>
        /// <param name="updateAction">Action to update the status.</param>
        private void UpdateProcessingStatus(Func<Core.Models.Search.SearchProcessingStatus, Core.Models.Search.SearchProcessingStatus> updateAction)
        {
            Core.Models.Search.SearchProcessingStatus updatedStatus;
            
            lock (_processingStatusLock)
            {
                updatedStatus = updateAction(_processingStatus);
                _processingStatus = updatedStatus;
            }
            
            // Raise the event outside the lock to avoid deadlocks
            ProcessingStatusChanged?.Invoke(this, updatedStatus);
            
            _logger.LogDebug("Search indexer processing status updated: {Operation}, IsProcessing: {IsProcessing}, Progress: {Progress}%",
                updatedStatus.CurrentOperation, updatedStatus.IsProcessing, updatedStatus.ProgressPercentage);
        }

        // Renamed method and changed return type
        private SearchableContent DocumentToSearchableContent(Document doc)
        {
            // Map standard fields
            var content = new SearchableContent
            {
                Id = doc.Get(IdField),
                MediaId = doc.Get(IdField), // Assuming Id is MediaId
                Title = doc.Get(TitleField),
                // FilePath is read-only and derived from MediaId
                Description = doc.Get(DescriptionField),
                Transcript = doc.Get(TranscriptField),
                ThumbnailUrl = doc.Get("ThumbnailUrl"), // Keep reading thumbnail URL if needed
                Metadata = new Dictionary<string, string>(), // Initialize Metadata
                // Handle numeric fields (assuming stored as Int64/Double)
                CreatedAt = new DateTime(doc.GetField(CreatedAtTicksField)?.GetInt64Value() ?? 0L, DateTimeKind.Utc),
                Duration = TimeSpan.FromSeconds(doc.GetField(DurationSecondsField)?.GetDoubleValue() ?? 0.0d),
                // Handle multi-valued fields
                Topics = doc.GetValues(TopicsField)?.ToList() ?? new List<string>(),
                // Keywords, Speakers, MediaType are now stored in Metadata

                 // Snippets and Timestamps are harder to reconstruct perfectly, depends on indexing strategy
                 Snippets = new List<SearchSnippet>(), // Placeholder
                 Timestamps = new List<SearchableTimestamp>() // Placeholder
            };

            // Populate Metadata from stored fields
            var storedMediaType = doc.Get(MediaTypeField);
            if (!string.IsNullOrEmpty(storedMediaType)) content.Metadata[MediaTypeField] = storedMediaType;

            var storedKeywords = doc.GetValues(KeywordsField);
            if (storedKeywords != null && storedKeywords.Length > 0) content.Metadata[KeywordsField] = string.Join(",", storedKeywords);

            var storedSpeakers = doc.GetValues(SpeakersField);
            if (storedSpeakers != null && storedSpeakers.Length > 0) content.Metadata[SpeakersField] = string.Join(",", storedSpeakers);

            // Reconstruction of Snippets/Timestamps might be complex
            // Example: If snippets were stored as separate fields
            // var snippetTexts = doc.GetValues("SnippetText");
            // var snippetStarts = doc.GetValues("SnippetStart");
            // ... reconstruct SearchSnippet objects ...

            return content;
        }

        private Document CreateDocument(string mediaId, SearchableContent content)
        {
            var doc = new Document();

            // Use StringField for IDs and fields not typically searched with analysis but used for filtering/retrieval
            // StoredField means the value is stored and can be retrieved directly from the document
            doc.Add(new StringField(IdField, mediaId, Field.Store.YES));
            doc.Add(new StoredField(FilePathField, content.FilePath)); // Use StoredField for read-only FilePath

            if (content.Metadata.TryGetValue(MediaTypeField, out var mediaType) && !string.IsNullOrEmpty(mediaType))
            {
                 doc.Add(new StringField(MediaTypeField, mediaType, Field.Store.YES));
            }

            // Use TextField for fields that need analysis (tokenization, lowercasing, etc.) for full-text search
            doc.Add(new TextField(FileNameField, content.FileName ?? string.Empty, Field.Store.YES)); // Use read-only FileName
            doc.Add(new TextField(TitleField, content.Title ?? string.Empty, Field.Store.YES));
            doc.Add(new TextField(DescriptionField, content.Description ?? string.Empty, Field.Store.YES));
            doc.Add(new TextField(TranscriptField, content.Transcript ?? string.Empty, Field.Store.YES)); // Store transcript for retrieval/highlighting

            // Add multi-valued fields (analyzed for search)
            if (content.Metadata.TryGetValue(KeywordsField, out var keywordsValue) && !string.IsNullOrEmpty(keywordsValue))
            {
                AddMultiValuedTextField(doc, KeywordsField, keywordsValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList());
            }
            AddMultiValuedTextField(doc, TopicsField, content.Topics);
            if (content.Metadata.TryGetValue(SpeakersField, out var speakersValue) && !string.IsNullOrEmpty(speakersValue))
            {
                 var speakersList = speakersValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                 AddMultiValuedTextField(doc, SpeakersField, speakersList);
                 // Add exact speaker field for filtering
                 foreach (var speaker in speakersList.Where(s => !string.IsNullOrWhiteSpace(s)))
                 {
                     doc.Add(new StringField(SpeakersField + "_exact", speaker.ToLowerInvariant(), Field.Store.NO)); // Not stored, just for filtering
                 }
            }

            // Add numeric fields for range queries (stored for retrieval)
            doc.Add(new Int64Field(CreatedAtTicksField, content.CreatedAt.ToUniversalTime().Ticks, Field.Store.YES));
            doc.Add(new DoubleField(DurationSecondsField, content.Duration.TotalSeconds, Field.Store.YES));

            // Add Thumbnail URL if present (Stored, not indexed for search unless needed)
            if (!string.IsNullOrEmpty(content.ThumbnailUrl))
            {
                 doc.Add(new StoredField("ThumbnailUrl", content.ThumbnailUrl));
            }

            // --- Removed Indexing Logic for AI Fields ---

            // Optional: Index Snippets / Timestamps
            // (Consider if needed and how best to store/query)

            return doc;
        }

        // Helper to add multi-valued text fields
        private void AddMultiValuedTextField(Document doc, string fieldName, List<string>? values)
        {
            if (values == null) return;
            foreach (var value in values.Where(v => !string.IsNullOrWhiteSpace(v)))
            {
                 // Add as TextField for analysis and searching
                 doc.Add(new TextField(fieldName, value, Field.Store.YES));
            }
        }

        private Task EnsureWriterCommittedAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                lock (_writerLock) { _writer.Commit(); }
                _logger.LogDebug("IndexWriter committed changes.");
            }, cancellationToken);
        }

        /// <summary>
        /// Gets all facet values for a specific field.
        /// </summary>
        public async Task<Dictionary<string, int>> GetFacetsAsync(string facetField, CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.Run(() =>
                {
                    lock (_writerLock)
                    {
                        _writer.Commit();
                    }
                });

                using var reader = DirectoryReader.Open(_directory);

                // Facet implementation simplified - consider using Lucene Facets module for efficiency
                var terms = new Dictionary<string, int>();

                var fields = MultiFields.GetFields(reader);
                if (fields == null) return terms;

                // Get a TermsEnum using the newer GetEnumerator method instead of GetIterator
                var termsEnum = fields.GetTerms(facetField)?.GetEnumerator();
                if (termsEnum == null) return terms;

                // Use MoveNext and Term pattern instead of the obsolete Next() method
                while (termsEnum.MoveNext())
                {
                    string term = termsEnum.Term.Utf8ToString();
                    if (!string.IsNullOrEmpty(term))
                    {
                        terms[term] = (int)termsEnum.TotalTermFreq; // Use TotalTermFreq for count
                    }
                }

                return terms;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facets for field: {FacetField}", facetField);
                return new Dictionary<string, int>();
            }
        }

        /// <summary>
        /// Disposes the resources used by the indexer.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the resources used by the indexer.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _logger.LogInformation("Disposing Lucene IndexWriter...");
                    lock (_writerLock)
                    {
                        _writer?.Commit();
                        _writer?.Dispose();
                    }
                    _directory?.Dispose();
                    _analyzer?.Dispose();
                    _logger.LogInformation("Lucene IndexWriter disposed.");
                }
                _disposed = true;
            }
        }

        private Document ConvertToDocument(SearchableContent content)
        {
            // Correctly call CreateDocument
            return CreateDocument(content.Id ?? content.MediaId ?? Guid.NewGuid().ToString(), content);
        }

        private SearchableContent MapDocumentToContent(Document doc, float score)
        {
            var mappedContent = new SearchableContent
            {
                Id = doc.Get(IdField),
                MediaId = doc.Get(IdField), // Assume Id is MediaId for reconstruction
                Title = doc.Get(TitleField),
                Description = doc.Get(DescriptionField),
                Transcript = doc.Get(TranscriptField),
                ThumbnailUrl = doc.Get("ThumbnailUrl"),
                Metadata = new Dictionary<string, string>(),
                Topics = doc.GetValues(TopicsField)?.ToList() ?? new List<string>(),
                Score = score
                // FilePath and FileName are read-only and derived from MediaId
            };

            var storedMediaType = doc.Get(MediaTypeField);
            if (!string.IsNullOrEmpty(storedMediaType)) mappedContent.Metadata[MediaTypeField] = storedMediaType;

            var storedKeywords = doc.GetValues(KeywordsField);
            if (storedKeywords != null && storedKeywords.Length > 0) mappedContent.Metadata[KeywordsField] = string.Join(",", storedKeywords);

            var storedSpeakers = doc.GetValues(SpeakersField);
            if (storedSpeakers != null && storedSpeakers.Length > 0) mappedContent.Metadata[SpeakersField] = string.Join(",", storedSpeakers);

            var createdAtTicks = doc.GetField(CreatedAtTicksField)?.GetInt64Value();
            if (createdAtTicks.HasValue) mappedContent.CreatedAt = new DateTime(createdAtTicks.Value, DateTimeKind.Utc);

            var durationSeconds = doc.GetField(DurationSecondsField)?.GetDoubleValue();
            if (durationSeconds.HasValue) mappedContent.Duration = TimeSpan.FromSeconds(durationSeconds.Value);

            // Reconstruction of Snippets and Timestamps might require more complex logic
            // based on how they were indexed (if at all).
            mappedContent.Snippets = new List<SearchSnippet>(); // Placeholder
            mappedContent.Timestamps = new List<SearchableTimestamp>(); // Placeholder

            return mappedContent;
        }
    }
}
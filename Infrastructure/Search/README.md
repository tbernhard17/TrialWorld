# TrialWorld Search Engine (Lucene Implementation)

This directory contains the infrastructure implementation for the TrialWorld search service using Lucene.NET. It provides full-text search capabilities across indexed media content and associated metadata like transcripts and keywords.

## Architecture

This implementation provides the concrete classes for the search interfaces defined in the Core layer:

1.  **Core Layer Dependencies**:
    *   `ISearchService`: Main interface for search operations.
    *   `ISearchIndexer`: Interface for low-level indexing operations.
    *   Various models from `TrialWorld.Core.Models` (e.g., `MediaMetadata`, `SearchResults`, `SearchableContent`, `TranscriptionResult`).

2.  **Infrastructure Layer (`Infrastructure/Search`)**:
    *   `LuceneSearchEngine`: Implements `ISearchService` using Lucene.NET. Handles query parsing, filter application, and result mapping.
    *   `LuceneSearchIndexer`: Implements `ISearchIndexer` using Lucene.NET. Manages the Lucene index (creating, updating, deleting documents).
    *   `SearchOptions`: Configuration class for the Lucene service (index path, etc.).
    *   `SearchServiceRegistration`: Dependency Injection setup.

## Features

*   **Full-text search**: Across media metadata (title, description) and extracted data (transcripts, topics, keywords, speakers).
*   **Filtering**: Supports filtering search results by:
    *   Speakers
    *   Keywords
    *   Topics
    *   Media Creation Date Range
    *   Media Duration Range
*   **Configurable Indexing**: Options for index location, batch size, and startup behavior.

## Using the Search Engine

### Basic Search

Inject `ISearchService` and use `SearchAsync`.

```csharp
// Assume ISearchService searchService is injected

var query = new TrialWorld.Core.Models.Search.SearchQuery
{
    Text = "search terms",
    PageNumber = 1,
    PageSize = 20,
    Filters = new Dictionary<string, string>(), // Add filters as needed
    SortBy = "" // Empty for relevance sort
};

var results = await searchService.SearchAsync(query);
// Process results.Items
```

### Advanced Search with Filters

Filters are passed via the `Filters` dictionary in the `SearchQuery` object. The `LuceneSearchEngine` translates these into Lucene filters.

Supported Filter Keys:

*   `speakers`: Comma-separated list of speaker names.
*   `keywords`: Comma-separated list of keywords.
*   `topics`: Comma-separated list of topics.
*   `date_start`: Start date/time in ISO 8601 format (e.g., `2024-01-01T00:00:00Z`).
*   `date_end`: End date/time in ISO 8601 format.
*   `duration_min_seconds`: Minimum duration in seconds (e.g., `60.5`).
*   `duration_max_seconds`: Maximum duration in seconds.

```csharp
var query = new TrialWorld.Core.Models.Search.SearchQuery
{
    Text = "relevant discussion",
    PageNumber = 1,
    PageSize = 10,
    Filters = new Dictionary<string, string>
    {
        { "speakers", "Speaker A,Speaker B" },
        { "date_start", "2024-03-01T00:00:00Z" },
        { "duration_min_seconds", "300" } // Minimum 5 minutes
    },
    SortBy = ""
};

var results = await searchService.SearchAsync(query);
// Process results
```

### Indexing Media

The `IndexMediaAsync` method accepts media metadata and transcription results to create/update a document in the search index.

```csharp
// Assume ISearchService searchService is injected
// Assume CoreMediaMetadata metadata and TranscriptionResult transcript are available

var success = await searchService.IndexMediaAsync(
    mediaId: "unique-media-id-123",
    mediaMetadata: metadata,
    transcription: transcript // Provide transcription data if available
);

if (success)
{
    // Media indexed/updated
}
```

## Configuration

Configure the search service in your `appsettings.json`:

```json
{
  "Search": {
    "IndexPath": "SearchIndex", // Relative or absolute path
    "IndexDirectory": "C:/WorldofTrials/SearchIndex", // Ensure this is correct
    "DefaultMaxResults": 50,
    "UseInMemoryIndex": false, // Usually false for persistence
    "MediaExtensions": [ ".mp4", ".mov", ".avi", ".mkv", ".mp3", ".wav", ".m4a" ],
    "AutoUpdateIndex": false,
    "AutoUpdateIntervalMinutes": 60,
    "IncludePrivateDirectories": false,
    // "ExtractTopics": true, // This option was removed
    "EnableTranscription": true, // Controls if transcription data is processed during indexing
    "BuildIndexOnStartup": true,
    "UseIncrementalIndexing": true,
    "IndexBatchSize": 100,
    "ResultPreFetchFactor": 50 // How many extra results to fetch internally for potential future ranking/pagination logic
  }
}
```

**Note:** AI-driven features like semantic search, face/emotion filtering, and automated ranking/learning based on user feedback have been removed or disabled in this implementation. 
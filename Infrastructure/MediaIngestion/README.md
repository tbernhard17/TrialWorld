# Media Ingestion Service

The Media Ingestion Service is responsible for importing, processing, and analyzing media files within the TrialWorld system. This service manages the entire ingestion pipeline including thumbnail generation, emotion analysis, transcription, and search indexing.

## Features

- **Asynchronous Processing**: Handle multiple media files concurrently
- **Progress Tracking**: Monitor the status of ingestion tasks
- **Error Handling**: Automatic retry mechanism for failed operations
- **Event Notifications**: Receive updates when ingestion status changes
- **Media Analysis**: Generate thumbnails, analyze emotions, and transcribe speech
- **Search Indexing**: Automatically index processed media for full-text and semantic search

## Usage

### Basic Example

```csharp
// Inject the service via DI
public class MyController
{
    private readonly IMediaIngestionService _mediaIngestionService;

    public MyController(IMediaIngestionService mediaIngestionService)
    {
        _mediaIngestionService = mediaIngestionService;
    }

    public async Task ImportMediaFileAsync(string filePath)
    {
        // Process the media file
        var result = await _mediaIngestionService.ProcessMediaAsync(filePath);
        
        if (result.Status == MediaProcessingStatus.Completed)
        {
            Console.WriteLine($"Media file processed successfully. Job ID: {result.JobId}");
            Console.WriteLine($"Processed file saved to: {result.ProcessedFilePath}");
            Console.WriteLine($"Metadata saved to: {result.MetadataPath}");
            Console.WriteLine($"Transcript saved to: {result.TranscriptPath}");
            Console.WriteLine($"Generated thumbnails: {string.Join(", ", result.ThumbnailPaths)}");
            Console.WriteLine("Media content has been automatically indexed for search");
        }
        else
        {
            Console.WriteLine($"Processing failed: {string.Join(", ", result.Errors)}");
        }
    }
}
```

### Tracking Progress

```csharp
public class MediaMonitor
{
    private readonly IMediaIngestionService _mediaIngestionService;

    public MediaMonitor(IMediaIngestionService mediaIngestionService)
    {
        _mediaIngestionService = mediaIngestionService;
    }

    public async Task CheckStatusAsync(string jobId)
    {
        var status = await _mediaIngestionService.GetProcessingStatusAsync(jobId);
        Console.WriteLine($"Current status: {status}");
    }
}
```

### Managing Tasks

```csharp
public class MediaManager
{
    private readonly IMediaIngestionService _mediaIngestionService;

    public MediaManager(IMediaIngestionService mediaIngestionService)
    {
        _mediaIngestionService = mediaIngestionService;
    }

    public async Task CancelIngestionAsync(string jobId)
    {
        await _mediaIngestionService.CancelProcessingAsync(jobId);
        Console.WriteLine($"Job {jobId} cancelled.");
    }
}
```

## Integration with Search

The Media Ingestion Service now automatically indexes processed media content through the `MediaContentIndexerService`. This integration enables full-text and semantic search capabilities across your media collection.

### Search Indexing Process

1. When media processing is completed, all metadata, transcripts, and analysis results are automatically indexed
2. The Lucene search index is updated to include the new content
3. Media can be searched by text, topics, emotions, and other metadata

### Manual Indexing

If you need to manually index content, you can use the `IMediaContentIndexerService` directly:

```csharp
public class MediaIndexer
{
    private readonly IMediaContentIndexerService _indexer;

    public MediaIndexer(IMediaContentIndexerService indexer)
    {
        _indexer = indexer;
    }

    public async Task IndexMediaContentAsync(MediaProcessingResult result)
    {
        bool success = await _indexer.IndexMediaContentAsync(result);
        if (success)
        {
            Console.WriteLine("Content indexed successfully");
        }
        else
        {
            Console.WriteLine("Failed to index content");
        }
    }

    public async Task UpdateMediaIndexAsync(string mediaId, MediaProcessingResult result)
    {
        await _indexer.UpdateMediaContentAsync(mediaId, result);
    }

    public async Task DeleteFromIndexAsync(string mediaId)
    {
        await _indexer.DeleteMediaContentAsync(mediaId);
    }
}
```

## Configuration

The Media Ingestion Service uses the following dependencies:

- `IMediaProcessingPipeline`: Orchestrates the processing of media files
- `IThumbnailGenerator`: For thumbnail generation from images and videos
- `ITranscriptionService`: For speech-to-text conversion
- `IEmotionAnalysisService`: For emotion analysis
- `IMediaContentIndexerService`: For indexing processed content for search

## Performance

By default, the service processes files sequentially. This can be adjusted if needed by implementing a job queue with parallelism in the `MediaIngestionService` class. 
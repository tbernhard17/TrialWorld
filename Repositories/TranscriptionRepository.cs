using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.AssemblyAIDiagnostic.Models;
using TrialWorld.Infrastructure.Transcription.Configuration;

namespace TrialWorld.AssemblyAIDiagnostic.Repositories
{
    /// <summary>
    /// Repository for managing transcription data.
    /// </summary>
    public interface ITranscriptionRepository
    {
        /// <summary>
        /// Saves a transcript to the database.
        /// </summary>
        /// <param name="mediaFilePath">The media file path.</param>
        /// <param name="transcript">The transcript data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveTranscriptAsync(string mediaFilePath, MediaTranscript transcript, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a transcript from the database.
        /// </summary>
        /// <param name="mediaFilePath">The media file path.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The transcript data if found; otherwise, null.</returns>
        Task<MediaTranscript?> GetTranscriptAsync(string mediaFilePath, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all transcripts from the database.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list of transcripts.</returns>
        Task<IEnumerable<MediaTranscript>> GetAllTranscriptsAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Searches transcripts in the database.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list of matching transcripts.</returns>
        Task<IEnumerable<MediaTranscript>> SearchTranscriptsAsync(string searchTerm, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Rebuilds the search index.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RebuildSearchIndexAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Implementation of the transcription repository.
    /// </summary>
    public class TranscriptionRepository : ITranscriptionRepository
    {
        private readonly ILogger<TranscriptionRepository> _logger;
        private readonly string _transcriptionDatabasePath;
        private readonly string _searchIndexPath;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly SemaphoreSlim _databaseLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="TranscriptionRepository"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The configuration options.</param>
        public TranscriptionRepository(
            ILogger<TranscriptionRepository> logger,
            IOptions<AssemblyAIOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var config = options?.Value ?? throw new ArgumentNullException(nameof(options));
            
            // Set up database paths
            var basePath = config.TranscriptionDatabasePath ?? Path.Combine(Directory.GetCurrentDirectory(), "TranscriptionDatabase");
            _transcriptionDatabasePath = Path.Combine(basePath, "Transcripts");
            _searchIndexPath = Path.Combine(basePath, "SearchIndex");
            
            // Create directories if they don't exist
            Directory.CreateDirectory(_transcriptionDatabasePath);
            Directory.CreateDirectory(_searchIndexPath);
            
            // Configure JSON options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            
            _logger.LogInformation("TranscriptionRepository initialized with database path: {DatabasePath}", _transcriptionDatabasePath);
        }

        /// <inheritdoc/>
        public async Task SaveTranscriptAsync(string mediaFilePath, MediaTranscript transcript, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(mediaFilePath))
            {
                throw new ArgumentException("Media file path cannot be null or empty", nameof(mediaFilePath));
            }

            if (transcript == null)
            {
                throw new ArgumentNullException(nameof(transcript));
            }

            // Generate a unique file name for the transcript
            var fileName = GetTranscriptFileName(mediaFilePath);
            var filePath = Path.Combine(_transcriptionDatabasePath, fileName);

            _logger.LogInformation("Saving transcript for {MediaFile} to {TranscriptFile}", mediaFilePath, filePath);

            await _databaseLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Save the transcript to a file
                var json = JsonSerializer.Serialize(transcript, _jsonOptions);
                await File.WriteAllTextAsync(filePath, json, cancellationToken).ConfigureAwait(false);

                // Update the search index
                await UpdateSearchIndexAsync(mediaFilePath, transcript, cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Transcript saved successfully for {MediaFile}", mediaFilePath);
            }
            finally
            {
                _databaseLock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<MediaTranscript?> GetTranscriptAsync(string mediaFilePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(mediaFilePath))
            {
                throw new ArgumentException("Media file path cannot be null or empty", nameof(mediaFilePath));
            }

            // Generate the file name for the transcript
            var fileName = GetTranscriptFileName(mediaFilePath);
            var filePath = Path.Combine(_transcriptionDatabasePath, fileName);

            if (!File.Exists(filePath))
            {
                _logger.LogInformation("Transcript not found for {MediaFile}", mediaFilePath);
                return null;
            }

            _logger.LogInformation("Loading transcript for {MediaFile} from {TranscriptFile}", mediaFilePath, filePath);

            await _databaseLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Load the transcript from the file
                var json = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
                var transcript = JsonSerializer.Deserialize<MediaTranscript>(json, _jsonOptions);

                _logger.LogInformation("Transcript loaded successfully for {MediaFile}", mediaFilePath);
                return transcript;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transcript for {MediaFile}", mediaFilePath);
                return null;
            }
            finally
            {
                _databaseLock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MediaTranscript>> GetAllTranscriptsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all transcripts from {DatabasePath}", _transcriptionDatabasePath);

            var transcripts = new List<MediaTranscript>();

            await _databaseLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Get all transcript files
                var files = Directory.GetFiles(_transcriptionDatabasePath, "*.json");

                foreach (var file in files)
                {
                    try
                    {
                        // Load the transcript from the file
                        var json = await File.ReadAllTextAsync(file, cancellationToken).ConfigureAwait(false);
                        var transcript = JsonSerializer.Deserialize<MediaTranscript>(json, _jsonOptions);

                        if (transcript != null)
                        {
                            transcripts.Add(transcript);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error loading transcript from {TranscriptFile}", file);
                    }
                }

                _logger.LogInformation("Loaded {Count} transcripts", transcripts.Count);
                return transcripts;
            }
            finally
            {
                _databaseLock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MediaTranscript>> SearchTranscriptsAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                throw new ArgumentException("Search term cannot be null or empty", nameof(searchTerm));
            }

            _logger.LogInformation("Searching transcripts for '{SearchTerm}'", searchTerm);

            // Simple in-memory search for now
            var allTranscripts = await GetAllTranscriptsAsync(cancellationToken).ConfigureAwait(false);
            var results = new List<MediaTranscript>();

            foreach (var transcript in allTranscripts)
            {
                if (transcript.Text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(transcript);
                }
                else if (transcript.Segments != null)
                {
                    foreach (var segment in transcript.Segments)
                    {
                        if (segment.Text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        {
                            results.Add(transcript);
                            break;
                        }
                    }
                }
            }

            _logger.LogInformation("Found {Count} transcripts matching '{SearchTerm}'", results.Count, searchTerm);
            return results;
        }

        /// <inheritdoc/>
        public async Task RebuildSearchIndexAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Rebuilding search index");

            await _databaseLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Clear the search index
                if (Directory.Exists(_searchIndexPath))
                {
                    Directory.Delete(_searchIndexPath, true);
                }
                Directory.CreateDirectory(_searchIndexPath);

                // Get all transcripts
                var transcripts = await GetAllTranscriptsAsync(cancellationToken).ConfigureAwait(false);

                // Rebuild the index
                foreach (var transcript in transcripts)
                {
                    if (transcript.MediaFilePath != null)
                    {
                        await UpdateSearchIndexAsync(transcript.MediaFilePath, transcript, cancellationToken).ConfigureAwait(false);
                    }
                }

                _logger.LogInformation("Search index rebuilt successfully");
            }
            finally
            {
                _databaseLock.Release();
            }
        }

        /// <summary>
        /// Updates the search index for a transcript.
        /// </summary>
        /// <param name="mediaFilePath">The media file path.</param>
        /// <param name="transcript">The transcript data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task UpdateSearchIndexAsync(string mediaFilePath, MediaTranscript transcript, CancellationToken cancellationToken = default)
        {
            // For now, this is a simple implementation that just stores the transcript text
            // In a production system, this would use a proper search index like Lucene.NET
            
            var indexFileName = GetSearchIndexFileName(mediaFilePath);
            var indexFilePath = Path.Combine(_searchIndexPath, indexFileName);

            // Create a simple index entry with the transcript text
            var indexEntry = new
            {
                MediaFilePath = mediaFilePath,
                Text = transcript.Text,
                Segments = transcript.Segments?.Select(s => new { s.Text, s.Start, s.End })
            };

            // Save the index entry to a file
            var json = JsonSerializer.Serialize(indexEntry, _jsonOptions);
            await File.WriteAllTextAsync(indexFilePath, json, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the transcript file name for a media file.
        /// </summary>
        /// <param name="mediaFilePath">The media file path.</param>
        /// <returns>The transcript file name.</returns>
        private string GetTranscriptFileName(string mediaFilePath)
        {
            // Generate a unique file name based on the media file path
            var hash = mediaFilePath.GetHashCode().ToString("X8");
            var fileName = Path.GetFileNameWithoutExtension(mediaFilePath);
            return $"{fileName}_{hash}.json";
        }

        /// <summary>
        /// Gets the search index file name for a media file.
        /// </summary>
        /// <param name="mediaFilePath">The media file path.</param>
        /// <returns>The search index file name.</returns>
        private string GetSearchIndexFileName(string mediaFilePath)
        {
            // Generate a unique file name based on the media file path
            var hash = mediaFilePath.GetHashCode().ToString("X8");
            var fileName = Path.GetFileNameWithoutExtension(mediaFilePath);
            return $"{fileName}_{hash}.idx";
        }
    }
}

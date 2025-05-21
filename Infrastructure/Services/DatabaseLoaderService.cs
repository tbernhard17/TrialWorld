using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Transcription;

namespace TrialWorld.Infrastructure.Services
{
    /// <summary>
    /// Implementation of IDatabaseLoaderService that loads transcription data from files.
    /// </summary>
    public class DatabaseLoaderService : IDatabaseLoaderService
    {
        private readonly ILogger<DatabaseLoaderService> _logger;
        private readonly string _baseDataPath;
        
        /// <summary>
        /// Initializes a new instance of the DatabaseLoaderService class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public DatabaseLoaderService(ILogger<DatabaseLoaderService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Set default data path to a folder within the application base directory
            _baseDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            
            // Ensure the data directory exists
            if (!Directory.Exists(_baseDataPath))
            {
                Directory.CreateDirectory(_baseDataPath);
                _logger.LogInformation("Created data directory at {DataPath}", _baseDataPath);
            }
        }
        
        /// <summary>
        /// Loads the media database, optionally filtering by name pattern.
        /// </summary>
        /// <param name="namePattern">Optional name pattern for filtering.</param>
        /// <returns>List of media metadata items.</returns>
        public async Task<List<MediaMetadata>> LoadMediaDatabaseAsync(string? namePattern = null)
        {
            _logger.LogDebug("Loading media database with pattern: {Pattern}", namePattern ?? "ALL");
            
            var mediaPath = Path.Combine(_baseDataPath, "Media");
            if (!Directory.Exists(mediaPath))
            {
                _logger.LogWarning("Media directory not found at {MediaPath}", mediaPath);
                return new List<MediaMetadata>();
            }

            try
            {
                var mediaFiles = Directory.GetFiles(mediaPath, "*.json");
                var result = new List<MediaMetadata>();

                foreach (var file in mediaFiles)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);
                        var item = JsonSerializer.Deserialize<MediaMetadata>(json);
                        
                        if (item != null && (string.IsNullOrEmpty(namePattern) || 
                            item.FileName.Contains(namePattern, StringComparison.OrdinalIgnoreCase)))
                        {
                            result.Add(item);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error loading media file {FileName}", file);
                    }
                }

                _logger.LogInformation("Loaded {Count} media items", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading media database");
                return new List<MediaMetadata>();
            }
        }

        /// <summary>
        /// Loads the keyword database, optionally filtering by name pattern.
        /// </summary>
        /// <param name="namePattern">Optional name pattern for filtering.</param>
        /// <returns>List of keyword data items.</returns>
        public async Task<List<KeywordData>> LoadKeywordDatabaseAsync(string? namePattern = null)
        {
            _logger.LogDebug("Loading keyword database with pattern: {Pattern}", namePattern ?? "ALL");
            
            var keywordPath = Path.Combine(_baseDataPath, "Keywords");
            if (!Directory.Exists(keywordPath))
            {
                _logger.LogWarning("Keywords directory not found at {KeywordPath}", keywordPath);
                return new List<KeywordData>();
            }

            try
            {
                var keywordFiles = Directory.GetFiles(keywordPath, "*.json");
                var result = new List<KeywordData>();

                foreach (var file in keywordFiles)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);
                        var item = JsonSerializer.Deserialize<KeywordData>(json);
                        
                        if (item != null && (string.IsNullOrEmpty(namePattern) || 
                            item.Text.Contains(namePattern, StringComparison.OrdinalIgnoreCase)))
                        {
                            result.Add(item);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error loading keyword file {FileName}", file);
                    }
                }

                _logger.LogInformation("Loaded {Count} keyword items", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading keyword database");
                return new List<KeywordData>();
            }
        }

        /// <summary>
        /// Loads the face database, optionally filtering by name pattern.
        /// </summary>
        /// <param name="namePattern">Optional name pattern for filtering.</param>
        /// <returns>List of face data items.</returns>
        public async Task<List<FaceData>> LoadFaceDatabaseAsync(string? namePattern = null)
        {
            _logger.LogDebug("Loading face database with pattern: {Pattern}", namePattern ?? "ALL");
            
            var facePath = Path.Combine(_baseDataPath, "Faces");
            if (!Directory.Exists(facePath))
            {
                _logger.LogWarning("Faces directory not found at {FacePath}", facePath);
                return new List<FaceData>();
            }

            try
            {
                var faceFiles = Directory.GetFiles(facePath, "*.json");
                var result = new List<FaceData>();

                foreach (var file in faceFiles)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);
                        var item = JsonSerializer.Deserialize<FaceData>(json);
                        
                        if (item != null && (string.IsNullOrEmpty(namePattern) || 
                            item.Id.Contains(namePattern, StringComparison.OrdinalIgnoreCase)))
                        {
                            result.Add(item);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error loading face file {FileName}", file);
                    }
                }

                _logger.LogInformation("Loaded {Count} face items", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading face database");
                return new List<FaceData>();
            }
        }

        /// <summary>
        /// Gets a list of available transcript files.
        /// </summary>
        /// <returns>List of transcript file information.</returns>
        public async Task<List<TranscriptFileInfo>> GetAvailableTranscriptsAsync()
        {
            _logger.LogDebug("Getting available transcripts");
            await Task.Yield(); // Make this properly async
            
            var transcriptsPath = Path.Combine(_baseDataPath, "Transcripts");
            if (!Directory.Exists(transcriptsPath))
            {
                _logger.LogWarning("Transcripts directory not found at {TranscriptsPath}", transcriptsPath);
                return new List<TranscriptFileInfo>();
            }

            try
            {
                var transcriptFiles = Directory.GetFiles(transcriptsPath, "*.json");
                var result = new List<TranscriptFileInfo>();

                foreach (var file in transcriptFiles)
                {
                    try
                    {
                        // Extract basic info from filename
                        var fileName = Path.GetFileName(file);
                        var fileInfo = new FileInfo(file);
                        
                        result.Add(new TranscriptFileInfo
                        {
                            FileName = fileName,
                            FullPath = file,
                            LastModified = fileInfo.LastWriteTime,
                            SizeBytes = fileInfo.Length,
                            // Set relative path as a fallback
                            RelativePath = fileName,
                            // Determine format based on file extension
                            Format = Path.GetExtension(file).ToLowerInvariant() switch
                            {
                                ".json" => TranscriptFormat.Json,
                                ".srt" => TranscriptFormat.Srt,
                                ".txt" => TranscriptFormat.Text,
                                _ => TranscriptFormat.Unknown
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing transcript file {FileName}", file);
                    }
                }

                // Sort by last modified date, newest first
                result = result.OrderByDescending(t => t.LastModified).ToList();
                
                _logger.LogInformation("Found {Count} transcript files", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available transcripts");
                return new List<TranscriptFileInfo>();
            }
        }

        /// <summary>
        /// Loads a transcript from a file.
        /// </summary>
        /// <param name="filePath">Path to the transcript file.</param>
        /// <returns>The loaded transcript, or null if loading fails.</returns>
        public async Task<TrialWorld.Core.Models.Transcription.Transcript?> LoadTranscriptFileAsync(string filePath)
        {
            _logger.LogDebug("Loading transcript file: {FilePath}", filePath);
            
            if (string.IsNullOrEmpty(filePath))
            {
                _logger.LogError("Transcript file path is null or empty");
                return null;
            }

            if (!File.Exists(filePath))
            {
                _logger.LogError("Transcript file not found: {FilePath}", filePath);
                return null;
            }

            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var coreTranscript = JsonSerializer.Deserialize<TrialWorld.Core.Models.MediaTranscript>(json, options);
                
                if (coreTranscript == null)
                {
                    _logger.LogError("Failed to deserialize transcript file: {FilePath}", filePath);
                    return null;
                }
                
                // Convert from Core.Models.MediaTranscript to Core.Models.Transcription.Transcript
                var transcriptionTranscript = new TrialWorld.Core.Models.Transcription.Transcript
                {
                    Id = coreTranscript.MediaId ?? Guid.NewGuid().ToString(), // Ensure ID is set if MediaId is null
                    Text = coreTranscript.FullText ?? string.Empty,
                    CreatedAt = coreTranscript.CreatedDate,
                    Segments = coreTranscript.Segments?.Select(s => new TrialWorld.Core.Models.Transcription.TranscriptSegment
                    {
                        Id = s.Id, // s is already TrialWorld.Core.Models.Transcription.TranscriptSegment
                        MediaId = s.MediaId,
                        Text = s.Text ?? string.Empty,
                        StartTime = s.StartTime, // Already double
                        EndTime = s.EndTime,   // Already double
                        Confidence = s.Confidence,
                        Speaker = s.Speaker, // s.SpeakerLabel is not a property of the new TranscriptSegment
                        Sentiment = s.Sentiment,
                        Words = s.Words
                    }).ToList() ?? new List<TrialWorld.Core.Models.Transcription.TranscriptSegment>()
                };
                
                _logger.LogInformation("Successfully loaded and converted transcript file: {FilePath}", filePath);
                return transcriptionTranscript;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transcript file: {FilePath}", filePath);
                return null;
            }
        }
    }
}

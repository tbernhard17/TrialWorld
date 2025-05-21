using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models.Transcription;
using TrialWorld.Core.Services;

namespace TrialWorld.Presentation.Services
{
    /// <summary>
    /// Adapter class that implements ITranscriptionDatabaseService by delegating to IDatabaseLoaderService
    /// </summary>
    public class TranscriptionDatabaseAdapter : ITranscriptionDatabaseService
    {
        private readonly IDatabaseLoaderService _databaseLoaderService;
        private readonly ILogger<TranscriptionDatabaseAdapter> _logger;

        /// <summary>
        /// Initializes a new instance of the TranscriptionDatabaseAdapter class
        /// </summary>
        /// <param name="databaseLoaderService">The database loader service to adapt</param>
        /// <param name="logger">The logger</param>
        public TranscriptionDatabaseAdapter(
            IDatabaseLoaderService databaseLoaderService,
            ILogger<TranscriptionDatabaseAdapter> logger)
        {
            _databaseLoaderService = databaseLoaderService ?? throw new ArgumentNullException(nameof(databaseLoaderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<TranscriptionResult?> GetTranscriptionAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting transcription with ID: {TranscriptionId}", id);
                
                // Get available transcripts
                var availableTranscripts = await _databaseLoaderService.GetAvailableTranscriptsAsync();
                var matchingFile = availableTranscripts.FirstOrDefault(t => t.TranscriptId == id);
                
                if (matchingFile == null)
                {
                    _logger.LogWarning("No transcription found with ID: {TranscriptionId}", id);
                    return null;
                }
                
                // Load the transcript file
                var transcript = await _databaseLoaderService.LoadTranscriptFileAsync(matchingFile.FullPath);
                
                if (transcript == null)
                {
                    _logger.LogWarning("Failed to load transcription file: {FilePath}", matchingFile.FullPath);
                    return null;
                }
                
                // Convert Transcript to TranscriptionResult
                return ConvertToTranscriptionResult(transcript, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transcription with ID: {TranscriptionId}", id);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TranscriptionResult>> GetAllTranscriptionsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting all transcriptions");
                
                // Get available transcripts
                var availableTranscripts = await _databaseLoaderService.GetAvailableTranscriptsAsync();
                var results = new List<TranscriptionResult>();
                
                foreach (var transcript in availableTranscripts)
                {
                    try
                    {
                        // Load the transcript file
                        var transcriptData = await _databaseLoaderService.LoadTranscriptFileAsync(transcript.FullPath);
                        
                        if (transcriptData != null)
                        {
                            // Convert Transcript to TranscriptionResult
                            var result = ConvertToTranscriptionResult(transcriptData, transcript.TranscriptId);
                            if (result != null)
                            {
                                results.Add(result);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error loading transcription file: {FilePath}", transcript.FullPath);
                    }
                }
                
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all transcriptions");
                return Enumerable.Empty<TranscriptionResult>();
            }
        }

        /// <inheritdoc/>
        public Task<bool> SaveTranscriptionAsync(TranscriptionResult transcription, CancellationToken cancellationToken = default)
        {
            // This is a read-only adapter, so we don't implement saving
            _logger.LogWarning("SaveTranscriptionAsync is not implemented in this adapter");
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> DeleteTranscriptionAsync(string id, CancellationToken cancellationToken = default)
        {
            // This is a read-only adapter, so we don't implement deletion
            _logger.LogWarning("DeleteTranscriptionAsync is not implemented in this adapter");
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> UpdateTranscriptionAsync(TranscriptionResult transcription, CancellationToken cancellationToken = default)
        {
            // This is a read-only adapter, so we don't implement updating
            _logger.LogWarning("UpdateTranscriptionAsync is not implemented in this adapter");
            return Task.FromResult(false);
        }
        
        /// <inheritdoc/>
        public async Task<IEnumerable<TranscriptionResult>> SearchTranscriptionsAsync(string query, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Searching transcriptions with query: {Query}", query);
                
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Enumerable.Empty<TranscriptionResult>();
                }
                
                // Get all transcriptions first
                var allTranscriptions = await GetAllTranscriptionsAsync(cancellationToken);
                
                // Perform a simple search on the text content
                // In a real implementation, this would use more sophisticated search techniques
                return allTranscriptions.Where(t => 
                    t.Text.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    t.Segments.Any(s => s.Text.Contains(query, StringComparison.OrdinalIgnoreCase)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching transcriptions with query: {Query}", query);
                return Enumerable.Empty<TranscriptionResult>();
            }
        }

        /// <summary>
        /// Converts a Transcript to a TranscriptionResult
        /// </summary>
        private TranscriptionResult ConvertToTranscriptionResult(TrialWorld.Core.Models.Transcription.Transcript? transcript, string originalId)
        {
            if (transcript == null) 
                throw new ArgumentNullException(nameof(transcript));

            var result = new TranscriptionResult 
            {
                Id = transcript.Id ?? originalId,
                TranscriptId = transcript.Id ?? originalId,
                Status = TranscriptionStatus.Completed,
                // Don't set Text directly as it's a read-only property
                // Instead set Transcript which Text property reads from
                Transcript = transcript.Text ?? string.Empty,
                Language = "en-US", // Default language
                AudioDuration = TimeSpan.FromSeconds(30), // Default duration
                Success = true,
                Confidence = 0.95 // Default confidence
            };
            
            // Set segments
            if (transcript.Segments != null)
            {
                result.Segments = transcript.Segments.ToList();
            }
            
            return result;
        }
    }
}

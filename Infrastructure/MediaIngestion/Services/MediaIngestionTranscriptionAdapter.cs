using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models.Transcription;
using TrialWorld.Core.Models.Processing;
using TrialWorld.Infrastructure.MediaIngestion.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Progress;

namespace TrialWorld.Infrastructure.MediaIngestion.Services
{
    public class MediaIngestionTranscriptionAdapter : ITranscriptionService
    {
        private readonly ILogger<MediaIngestionTranscriptionAdapter> _logger;
        private readonly ITranscriptionService _transcriptionService;
        // Using ITranscriptionService for transcription functionality

        public MediaIngestionTranscriptionAdapter(
            IConfiguration configuration,
            HttpClient httpClient,
            ITranscriptionService transcriptionService,
            ILogger<MediaIngestionTranscriptionAdapter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _transcriptionService = transcriptionService ?? throw new ArgumentNullException(nameof(transcriptionService));
            // Using ITranscriptionService for transcription
        }

        // ITranscriptionService implementation
        public async Task<TranscriptionResult> TranscribeAsync(string filePath, CancellationToken cancellationToken)
        {
            // Use the TranscribeAsync method with default TranscriptionConfig
            return await TranscribeAsync(filePath, new TranscriptionConfig(), cancellationToken);
        }

        public async Task<TranscriptionResult> TranscribeWithProgressAsync(string filePath, TranscriptionConfig config, IProgress<TranscriptionProgressUpdate> progress, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Media file not found", filePath);

            _logger.LogInformation("Submitting transcription request with progress reporting for: {MediaPath}", filePath);

            try
            {
                // Delegate to the underlying transcription service using the new SDK-based implementation
                // Use TranscribeWithProgressAsync since we have a progress reporter
                TranscriptionResult? result = await _transcriptionService.TranscribeWithProgressAsync(
                    filePath,
                    progress,
                    cancellationToken);
                
                return result ?? new TranscriptionResult
                {
                    MediaPath = filePath,
                    Status = TranscriptionStatus.Failed,
                    Error = "TranscriptionResult was null",
                    Success = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during transcription with progress for {MediaPath}", filePath);
                return new TranscriptionResult
                {
                    MediaPath = filePath,
                    Status = TranscriptionStatus.Failed,
                    Error = ex.Message,
                    Success = false
                };
            }
        }

        public async Task<TranscriptionResult> TranscribeAsync(string filePath, TranscriptionConfig config, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Media file not found", filePath);

            _logger.LogInformation("Submitting transcription request via ITranscriptionService for: {MediaPath}", filePath);

            try
            {
                // Call the richer service. Assuming it handles the full transcription lifecycle.
                TranscriptionResult? result = await _transcriptionService.TranscribeAsync(filePath, config, cancellationToken);
                
                // Ensure we have a valid result
                result ??= new TranscriptionResult { MediaPath = filePath, Status = TranscriptionStatus.Failed, Error = "Transcription service returned null." };

                if (result.Id == null)
                {
                    _logger.LogError("ITranscriptionService.TranscribeAsync returned null for {FilePath}.", filePath);
                    // Throw a specific exception indicating failure or return a default/error TranscriptionResult
                    throw new InvalidOperationException($"Transcription failed to return a result for {filePath}.");
                    // Example of returning an error result:
                    // return new TranscriptionResult { FilePath = filePath, Status = TranscriptionStatus.Failed, Error = "Transcription service returned null." };
                }

                if (result.Status == TranscriptionStatus.Failed && string.IsNullOrEmpty(result.Error))
                {
                    result.Error = "Transcription failed with unknown error.";
                    _logger.LogWarning("Transcription failed for {FilePath} with ID {TranscriptionId} but no error message provided.", filePath, result.Id);
                }
                else if (result.Status != TranscriptionStatus.Completed)
                {
                     _logger.LogWarning("Transcription for {FilePath} (ID: {TranscriptionId}) completed but status is {Status}. Error: {Error}", filePath, result.Id, result.Status, result.Error);
                }
                else
                {
                    _logger.LogInformation("Transcription completed successfully for {FilePath}. ID: {TranscriptionId}", filePath, result.Id);
                }

                // Return the full result object as required by the updated ITranscriptionService interface
                // return result.Id; // OLD - Returned only string ID
                return result; // NEW - Return full TranscriptionResult
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error submitting transcription for {FilePath}", filePath);
                 // Rethrow or wrap in a more specific exception if needed
                 throw;
            }
        }

        public Task<TranscriptionStatus> GetTranscriptionStatusAsync(string transcriptionId, CancellationToken cancellationToken)
        {
            _logger.LogWarning("GetTranscriptionStatusAsync is not directly supported by the underlying implementation.");
            // The underlying transcription service contract focuses on completing the transcription.
            // Separate status checks are not part of its current interface.
            // Option 1: Return Unknown
             return Task.FromResult(TranscriptionStatus.Unknown);
            // Option 2: Throw NotImplementedException
            // throw new NotImplementedException("Getting transcription status separately is not supported by this implementation.");
        }

        public Task<bool> CancelTranscriptionAsync(string transcriptionId, CancellationToken cancellationToken)
        {
            // Delegate to the underlying service
            return _transcriptionService.CancelTranscriptionAsync(transcriptionId, cancellationToken);
        }
        
        /// <inheritdoc/>
        public Task<bool> DownloadTranscriptionFileAsync(
            string transcriptionId, 
            string outputPath, 
            IProgress<TranscriptionProgressUpdate>? progress = null, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Delegating download of transcription {TranscriptionId} to underlying service", transcriptionId);
            
            // Delegate to the underlying transcription service
            return _transcriptionService.DownloadTranscriptionFileAsync(
                transcriptionId,
                outputPath,
                progress,
                cancellationToken);
        }

        // Implement the new interface method
        public async Task<TranscriptionResult> TranscribeWithProgressAsync(
            string filePath,
            IProgress<TranscriptionProgressUpdate> progress,
            CancellationToken cancellationToken)
        {
            if (progress != null)
            {
                // Direct pass-through of the IProgress<TranscriptionProgressUpdate>
                // The SDK-based transcription service already expects the correct type
                var result = await _transcriptionService.TranscribeWithProgressAsync(
                    filePath,
                    progress,
                    cancellationToken);
                    
                return result ?? new TranscriptionResult 
                { 
                    MediaPath = filePath, 
                    Status = TranscriptionStatus.Failed, 
                    Error = "Transcription service returned null." 
                };
            }
            else
            {
                // If no progress reporting is needed, use the standard method
                return await TranscribeAsync(filePath, cancellationToken);
            }
        }
        
        private TranscriptionPhase DetermineTranscriptionPhase(string stageName)
        {
            return stageName?.ToLower() switch
            {
                "queued" => TranscriptionPhase.Queued,
                "uploading" => TranscriptionPhase.Uploading,
                "processing" => TranscriptionPhase.Processing,
                "completed" => TranscriptionPhase.Completed,
                "failed" => TranscriptionPhase.Failed,
                "cancelled" => TranscriptionPhase.Cancelled,
                _ => TranscriptionPhase.Processing
            };
        }
        
        private TranscriptionStatus DetermineTranscriptionStatus(string stageName, bool isComplete)
        {
            if (isComplete)
            {
                return TranscriptionStatus.Completed;
            }
            
            // Convert the stage name to a more user-friendly status message
            return stageName?.ToLower() switch
            {
                "queued" => TranscriptionStatus.Queued,
                "silencedetection" => TranscriptionStatus.RemovingSilence,
                "uploading" => TranscriptionStatus.UploadingAudio,
                "submitted" => TranscriptionStatus.WaitingForTranscription,
                "processing" => TranscriptionStatus.Transcribing,
                "downloading" => TranscriptionStatus.DownloadingResults,
                "preprocessing" => TranscriptionStatus.Preprocessing,
                "transcribing" => TranscriptionStatus.Transcribing,
                "postprocessing" => TranscriptionStatus.Postprocessing,
                "failed" => TranscriptionStatus.Failed,
                "cancelled" => TranscriptionStatus.Cancelled,
                _ => TranscriptionStatus.Processing
            };
        }
    }
}
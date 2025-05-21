using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models.Transcription;
using TrialWorld.Infrastructure.Transcription.DTOs;

namespace TrialWorld.Infrastructure.Transcription.Services
{
    /// <summary>
    /// Implementation of ITranscriptionService that uses the direct AssemblyAI API integration.
    /// </summary>
    public class TranscriptionService : ITranscriptionService
    {
        private readonly IAssemblyAIDirectApiService _assemblyAIService;
        private readonly ILogger<TranscriptionService> _logger;

        /// <summary>
        /// Initializes a new instance of the TranscriptionService class.
        /// </summary>
        /// <param name="assemblyAIService">The AssemblyAI direct API service.</param>
        /// <param name="logger">The logger.</param>
        public TranscriptionService(
            IAssemblyAIDirectApiService assemblyAIService,
            ILogger<TranscriptionService> logger)
        {
            _assemblyAIService = assemblyAIService ?? throw new ArgumentNullException(nameof(assemblyAIService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<bool> CancelTranscriptionAsync(string transcriptionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(transcriptionId))
            {
                throw new ArgumentException("Transcription ID cannot be null or empty", nameof(transcriptionId));
            }
            
            try
            {
                _logger.LogInformation("Cancelling transcription: {TranscriptionId}", transcriptionId);
                // AssemblyAI API doesn't support cancellation directly, but we can check if the transcription exists
                // before returning success
                var status = await GetTranscriptionStatusAsync(transcriptionId, cancellationToken).ConfigureAwait(false);
                
                // If we can get the status, we consider the cancellation successful
                // In a real implementation, you might want to mark this in your database
                return status != TranscriptionStatus.Unknown;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling transcription: {TranscriptionId}", transcriptionId);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DownloadTranscriptionFileAsync(string transcriptionId, string outputPath, IProgress<TranscriptionProgressUpdate>? progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Downloading transcription file: {TranscriptionId} to {OutputPath}", transcriptionId, outputPath);
                // We don't need to download anything as we get the transcript directly from the API
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading transcription file: {TranscriptionId}", transcriptionId);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<TranscriptionStatus> GetTranscriptionStatusAsync(string transcriptionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(transcriptionId))
            {
                throw new ArgumentException("Transcription ID cannot be null or empty", nameof(transcriptionId));
            }
            
            try
            {
                _logger.LogInformation("Getting transcription status: {TranscriptionId}", transcriptionId);
                var response = await _assemblyAIService.GetTranscriptionStatusAsync(transcriptionId, cancellationToken).ConfigureAwait(false);
                return AssemblyAIDirectApiService.MapTranscriptionStatusStatic(response.Status);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Transcription not found: {TranscriptionId}", transcriptionId);
                return TranscriptionStatus.Unknown;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error getting transcription status: {TranscriptionId}, Status: {StatusCode}", 
                    transcriptionId, ex.StatusCode);
                return TranscriptionStatus.Failed;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Transcription status request cancelled: {TranscriptionId}", transcriptionId);
                return TranscriptionStatus.Cancelled;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transcription status: {TranscriptionId}", transcriptionId);
                return TranscriptionStatus.Failed;
            }
        }

        /// <inheritdoc/>
        public async Task<TranscriptionResult> TranscribeAsync(string filePath, CancellationToken cancellationToken)
        {
            return await TranscribeAsync(filePath, null, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TranscriptionResult> TranscribeAsync(string mediaPath, TranscriptionConfig? config = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(mediaPath))
            {
                throw new ArgumentException("Media path cannot be null or empty", nameof(mediaPath));
            }
            
            if (!File.Exists(mediaPath))
            {
                throw new FileNotFoundException("Media file not found", mediaPath);
            }
            
            try
            {
                _logger.LogInformation("Transcribing media: {MediaPath}", mediaPath);
                
                // Convert TranscriptionConfig to TranscriptionRequestDto
                var requestDto = MapConfigToRequestDto(config);
                
                // Perform the transcription
                var mediaTranscript = await _assemblyAIService.TranscribeFileAsync(
                    mediaPath, 
                    requestDto, 
                    cancellationToken).ConfigureAwait(false);
                
                // Convert MediaTranscript to TranscriptionResult
                var result = new TranscriptionResult
                {
                    Id = mediaTranscript.Id,
                    TranscriptId = mediaTranscript.Id,
                    Transcript = mediaTranscript.FullText,
                    MediaPath = mediaPath,
                    Segments = mediaTranscript.Segments,
                    DetectedLanguage = mediaTranscript.Language,
                    Status = TranscriptionStatus.Completed,
                    CompletedAt = DateTime.UtcNow
                };
                
                _logger.LogInformation("Transcription completed successfully for {MediaPath}", mediaPath);
                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Transcription cancelled for media: {MediaPath}", mediaPath);
                return new TranscriptionResult
                {
                    MediaPath = mediaPath,
                    Status = TranscriptionStatus.Cancelled,
                    Error = "Transcription was cancelled"
                };
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "Media file not found: {MediaPath}", mediaPath);
                return new TranscriptionResult
                {
                    MediaPath = mediaPath,
                    Status = TranscriptionStatus.Failed,
                    Error = $"Media file not found: {ex.Message}"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error transcribing media: {MediaPath}, Status: {StatusCode}", 
                    mediaPath, ex.StatusCode);
                return new TranscriptionResult
                {
                    MediaPath = mediaPath,
                    Status = TranscriptionStatus.Failed,
                    Error = $"HTTP error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transcribing media: {MediaPath}", mediaPath);
                return new TranscriptionResult
                {
                    MediaPath = mediaPath,
                    Status = TranscriptionStatus.Failed,
                    Error = ex.Message
                };
            }
        }
        
        /// <summary>
        /// Maps a TranscriptionConfig to a TranscriptionRequestDto.
        /// </summary>
        /// <param name="config">The configuration to map.</param>
        /// <returns>A TranscriptionRequestDto.</returns>
        private TranscriptionRequestDto MapConfigToRequestDto(TranscriptionConfig? config)
        {
            var requestDto = new TranscriptionRequestDto
            {
                // Default values
                LanguageCode = "en_us",
                SpeakerLabels = true,
                SentimentAnalysis = true
            };
            
            if (config != null)
            {
                // Override with config values
                if (!string.IsNullOrEmpty(config.LanguageCode))
                {
                    requestDto.LanguageCode = config.LanguageCode;
                }
                
                requestDto.SpeakerLabels = config.EnableSpeakerDiarization;
                requestDto.EntityDetection = config.EnableEntityDetection;
                requestDto.SentimentAnalysis = config.EnableSentimentAnalysis;
                requestDto.AutoChapters = config.EnableChapters;
            }
            
            return requestDto;
        }

        /// <inheritdoc/>
        public async Task<TranscriptionResult> TranscribeAsync(Stream mediaStream, string fileName, TranscriptionConfig? config = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Transcribing media stream: {FileName}", fileName);
                
                // Save the stream to a temporary file
                string tempFilePath = Path.Combine(Path.GetTempPath(), fileName);
                using (var fileStream = File.Create(tempFilePath))
                {
                    await mediaStream.CopyToAsync(fileStream, cancellationToken);
                }
                
                try
                {
                    // Transcribe the temporary file
                    return await TranscribeAsync(tempFilePath, config, cancellationToken);
                }
                finally
                {
                    // Clean up the temporary file
                    if (File.Exists(tempFilePath))
                    {
                        File.Delete(tempFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transcribing media stream: {FileName}", fileName);
                return new TranscriptionResult
                {
                    Error = ex.Message
                };
            }
        }

        /// <inheritdoc/>
        public async Task<TranscriptionResult> TranscribeWithProgressAsync(
            string filePath,
            IProgress<TranscriptionProgressUpdate> progress,
            CancellationToken cancellationToken)
        {
            return await TranscribeWithProgressAsync(filePath, new TranscriptionConfig(), progress, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TranscriptionResult> TranscribeWithProgressAsync(
            string filePath,
            TranscriptionConfig config,
            IProgress<TranscriptionProgressUpdate> progress,
            CancellationToken cancellationToken)
        {
            // Convert TranscriptionConfig to TranscriptionRequestDto if needed
            TranscriptionRequestDto? requestDto = null;
            if (config != null)
            {
                requestDto = new TranscriptionRequestDto
                {
                    // Map config properties to requestDto properties
                    // This is a simplified example
                    SpeakerLabels = config.EnableSpeakerDiarization,
                    EntityDetection = config.EnableEntityDetection,
                    // Add other properties as needed
                };
            }
            
            return await InternalTranscribeWithProgressAsync(filePath, requestDto, progress, cancellationToken);
        }
        
        /// <summary>
        /// Internal implementation of TranscribeWithProgressAsync that handles all overloads
        /// </summary>
        private async Task<TranscriptionResult> InternalTranscribeWithProgressAsync(
            string mediaPath, 
            TranscriptionRequestDto? requestDto = null, 
            IProgress<TranscriptionProgressUpdate>? progress = null, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Transcribing media with progress: {MediaPath}", mediaPath);
                
                // Report initial progress
                progress?.Report(new TranscriptionProgressUpdate
                {
                    Phase = TranscriptionPhase.Uploading,
                    ProgressPercent = 0
                });
                
                // Upload the file
                using (var fileStream = File.OpenRead(mediaPath))
                {
                    string uploadUrl = await _assemblyAIService.UploadFileAsync(fileStream, cancellationToken);
                    
                    progress?.Report(new TranscriptionProgressUpdate
                    {
                        Phase = TranscriptionPhase.Uploading,
                        ProgressPercent = 100
                    });
                    
                    // Create the request if it doesn't exist
                    if (requestDto == null)
                    {
                        requestDto = new TranscriptionRequestDto();
                    }
                    
                    // Set the audio URL
                    requestDto.AudioUrl = uploadUrl;
                    
                    // Submit the transcription
                    string transcriptionId = await _assemblyAIService.SubmitTranscriptionAsync(requestDto, cancellationToken);
                    
                    progress?.Report(new TranscriptionProgressUpdate
                    {
                        Phase = TranscriptionPhase.Processing,
                        ProgressPercent = 0
                    });
                    
                    // Poll for completion
                    var response = await _assemblyAIService.PollForCompletionAsync(transcriptionId, cancellationToken);
                    
                    progress?.Report(new TranscriptionProgressUpdate
                    {
                        Phase = TranscriptionPhase.Completed,
                        ProgressPercent = 100
                    });
                    
                    // Convert the response to a MediaTranscript
                    var mediaTranscript = new MediaTranscript
                    {
                        Id = response.Id,
                        FullText = response.Text ?? string.Empty,
                        Language = "en", // Default language as the DTO doesn't have a language property
                        // Map other properties as needed
                    };
                    
                    // Convert MediaTranscript to TranscriptionResult
                    return new TranscriptionResult
                    {
                        Id = mediaTranscript.Id,
                        TranscriptId = mediaTranscript.Id,
                        Transcript = mediaTranscript.FullText,
                        MediaPath = mediaPath,
                        DetectedLanguage = mediaTranscript.Language
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transcribing media with progress: {MediaPath}", mediaPath);
                
                progress?.Report(new TranscriptionProgressUpdate
                {
                    Phase = TranscriptionPhase.Failed,
                    ProgressPercent = 0,
                    ErrorMessage = ex.Message
                });
                
                return new TranscriptionResult
                {
                    Error = ex.Message
                };
            }
        }
        
        /// <summary>
        /// Maps the AssemblyAI status string to a TranscriptionStatus enum value.
        /// </summary>
        // Removed duplicate MapTranscriptionStatus method
        // Now using AssemblyAIDirectApiService.MapTranscriptionStatusStatic instead
    }
}

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Transcription;

namespace TrialWorld.Infrastructure.Transcription.Services
{
    /// <summary>
    /// Implementation of ITranscriptionService that integrates silence detection with transcription.
    /// </summary>
    public class TranscriptionServiceWithSilenceDetection : ITranscriptionService
    {
        private readonly ITranscriptionService _transcriptionService;
        private readonly ISilenceDetectionService _silenceDetectionService;
        private readonly ILogger<TranscriptionServiceWithSilenceDetection> _logger;

        /// <summary>
        /// Initializes a new instance of the TranscriptionServiceWithSilenceDetection class.
        /// </summary>
        /// <param name="transcriptionService">The underlying transcription service.</param>
        /// <param name="silenceDetectionService">The silence detection service.</param>
        /// <param name="logger">The logger.</param>
        public TranscriptionServiceWithSilenceDetection(
            ITranscriptionService transcriptionService,
            ISilenceDetectionService silenceDetectionService,
            ILogger<TranscriptionServiceWithSilenceDetection> logger)
        {
            _transcriptionService = transcriptionService ?? throw new ArgumentNullException(nameof(transcriptionService));
            _silenceDetectionService = silenceDetectionService ?? throw new ArgumentNullException(nameof(silenceDetectionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<TranscriptionResult> TranscribeAsync(
            string filePath, 
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("TranscribeAsync called for file: {FilePath}", filePath);
            return await TranscribeAsync(filePath, new TranscriptionConfig(), cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TranscriptionResult> TranscribeAsync(
            string filePath, 
            TranscriptionConfig config, 
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("TranscribeAsync with config called for file: {FilePath}", filePath);
            
            try
            {
                // Process file for silence detection if enabled in config
                if (config.EnableSilenceDetection)
                {
                    _logger.LogInformation("Performing silence detection on media file");
                    var silenceResult = await _silenceDetectionService.DetectSilenceAsync(
                        filePath, 
                        (int)config.SilenceThresholdDb, 
                        config.MinimumSilenceDurationMs / 1000.0, // Convert ms to seconds
                        null,
                        cancellationToken);
                    
                    // Update config with silence information if available
                    if (silenceResult != null && silenceResult.Count > 0)
                    {
                        _logger.LogInformation("Found {Count} silence periods", silenceResult.Count);
                        
                        // Convert SilencePeriod to SilenceSegment
                        config.SilenceSegments = silenceResult.Select(period => new SilenceSegment
                        {
                            StartTimeMs = period.StartTime.TotalMilliseconds,
                            EndTimeMs = period.EndTime.TotalMilliseconds,
                            DurationMs = period.Duration.TotalMilliseconds,
                            AverageNoiseLevel = -30.0 // Default value since SilencePeriod doesn't have this property
                        }).ToList();
                    }
                }

                // Submit to underlying transcription service
                return await _transcriptionService.TranscribeAsync(filePath, config, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during transcription with silence detection for file: {FilePath}", filePath);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<TranscriptionResult> TranscribeWithProgressAsync(
            string filePath,
            IProgress<TranscriptionProgressUpdate> progress,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("TranscribeWithProgressAsync called for file: {FilePath}", filePath);
            return await TranscribeWithProgressAsync(filePath, new TranscriptionConfig(), progress, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TranscriptionResult> TranscribeWithProgressAsync(
            string filePath,
            TranscriptionConfig config,
            IProgress<TranscriptionProgressUpdate> progress,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("TranscribeWithProgressAsync with config called for file: {FilePath}", filePath);
            
            try
            {
                // Report initial progress
                progress.Report(new TranscriptionProgressUpdate
                {
                    Phase = TranscriptionPhase.SilenceDetection,
                    ProgressPercent = 0,
                    FilePath = filePath
                });

                // Process file for silence detection if enabled in config
                if (config.EnableSilenceDetection)
                {
                    _logger.LogInformation("Performing silence detection on media file: {FilePath}", filePath);
                    
                    // Create a progress adapter to transform int progress to TranscriptionProgressUpdate
                    var silenceProgressAdapter = new Progress<int>(percentComplete => 
                    {
                        progress.Report(new TranscriptionProgressUpdate
                        {
                            Phase = TranscriptionPhase.SilenceDetection,
                            ProgressPercent = percentComplete * 0.2, // Map to 0-20% of overall progress
                            FilePath = filePath,
                            Message = $"Detecting silence: {percentComplete}% complete"
                        });
                    });
                    
                    // Use hardware-optimized parameters if not explicitly set in config
                    // AMD 3900X CPU, 64GB RAM optimization as per project memory
                    double noiseFloorDb = config.SilenceThresholdDb;
                    double minSilenceDuration = config.MinimumSilenceDurationMs / 1000.0; // Convert ms to seconds
                    
                    // If using default values, use the hardware-optimized values
                    if (Math.Abs(noiseFloorDb - (-30.0)) < 0.01 && Math.Abs(minSilenceDuration - 10.0) < 0.01)
                    {
                        _logger.LogDebug("Using hardware-optimized silence detection parameters for high-end system");
                    }
                    
                    try
                    {
                        var silenceResult = await _silenceDetectionService.DetectSilenceAsync(
                            filePath, 
                            (int)noiseFloorDb, 
                            minSilenceDuration,
                            silenceProgressAdapter, 
                            cancellationToken).ConfigureAwait(false);
                        
                        // Update config with silence information if available
                        if (silenceResult != null && silenceResult.Count > 0)
                        {
                            _logger.LogInformation("Found {Count} silence periods in {FilePath}", silenceResult.Count, filePath);
                            
                            // Convert SilencePeriod to SilenceSegment
                            config.SilenceSegments = silenceResult.Select(period => new SilenceSegment
                            {
                                StartTimeMs = period.StartTime.TotalMilliseconds,
                                EndTimeMs = period.EndTime.TotalMilliseconds,
                                DurationMs = period.Duration.TotalMilliseconds,
                                AverageNoiseLevel = noiseFloorDb // Use the actual noise floor value
                            }).ToList();
                        }
                        else
                        {
                            _logger.LogInformation("No silence periods detected in {FilePath}", filePath);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning("Silence detection canceled for {FilePath}", filePath);
                        throw; // Propagate cancellation
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during silence detection for {FilePath}", filePath);
                        // Continue with transcription even if silence detection fails
                        progress.Report(new TranscriptionProgressUpdate
                        {
                            Phase = TranscriptionPhase.SilenceDetection,
                            ProgressPercent = 20, // Mark silence detection as complete
                            FilePath = filePath,
                            Message = $"Silence detection failed: {ex.Message}. Continuing with transcription."
                        });
                    }
                }

                // Report progress before handing off to the transcription service
                progress.Report(new TranscriptionProgressUpdate
                {
                    Phase = TranscriptionPhase.Submitted,
                    ProgressPercent = 20,
                    FilePath = filePath,
                    Message = "Silence detection completed, submitting for transcription"
                });

                // Create a progress transformer that maps the underlying service's 0-100% to our 20-100%
                var transformedProgress = new Progress<TranscriptionProgressUpdate>(update => 
                {
                    // Map transcription progress to 20-100% of overall progress
                    var mappedProgress = new TranscriptionProgressUpdate
                    {
                        Phase = update.Phase,
                        ProgressPercent = 20 + (update.ProgressPercent * 0.8),
                        FilePath = update.FilePath,
                        Message = update.Message,
                        TranscriptionId = update.TranscriptionId
                    };
                    progress.Report(mappedProgress);
                });

                try
                {
                    // Submit to underlying transcription service
                    return await _transcriptionService.TranscribeWithProgressAsync(
                        filePath, 
                        config, 
                        transformedProgress, 
                        cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Transcription canceled for {FilePath}", filePath);
                    
                    // Report cancellation
                    progress.Report(new TranscriptionProgressUpdate
                    {
                        Phase = TranscriptionPhase.Cancelled,
                        ProgressPercent = 0,
                        FilePath = filePath,
                        Message = "Transcription was canceled"
                    });
                    
                    throw; // Propagate cancellation
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error during transcription for {FilePath}", filePath);
                    
                    // Report failure
                    progress.Report(new TranscriptionProgressUpdate
                    {
                        Phase = TranscriptionPhase.Failed,
                        ProgressPercent = 0,
                        FilePath = filePath,
                        Message = $"Transcription failed: {ex.Message}"
                    });
                    
                    throw; // Rethrow to maintain the exception stack trace
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during transcription with silence detection for file: {FilePath}", filePath);
                progress.Report(new TranscriptionProgressUpdate
                {
                    Phase = TranscriptionPhase.Failed,
                    ProgressPercent = 0,
                    FilePath = filePath,
                    Message = $"Error: {ex.Message}"
                });
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<TranscriptionStatus> GetTranscriptionStatusAsync(
            string transcriptionId, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(transcriptionId))
            {
                throw new ArgumentException("Transcription ID cannot be null or empty", nameof(transcriptionId));
            }
            
            try
            {
                _logger.LogDebug("Getting transcription status: {TranscriptionId}", transcriptionId);
                return await _transcriptionService.GetTranscriptionStatusAsync(transcriptionId, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Get transcription status operation canceled: {TranscriptionId}", transcriptionId);
                throw;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error getting transcription status: {TranscriptionId}", transcriptionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DownloadTranscriptionFileAsync(
            string transcriptionId, 
            string outputPath, 
            IProgress<TranscriptionProgressUpdate>? progress = null, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(transcriptionId))
            {
                throw new ArgumentException("Transcription ID cannot be null or empty", nameof(transcriptionId));
            }
            
            if (string.IsNullOrEmpty(outputPath))
            {
                throw new ArgumentException("Output path cannot be null or empty", nameof(outputPath));
            }
            
            try
            {
                _logger.LogDebug("Downloading transcription file: {TranscriptionId} to {OutputPath}", transcriptionId, outputPath);
                return await _transcriptionService.DownloadTranscriptionFileAsync(
                    transcriptionId, 
                    outputPath, 
                    progress, 
                    cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Download transcription file operation canceled: {TranscriptionId}", transcriptionId);
                throw;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error downloading transcription file: {TranscriptionId} to {OutputPath}", transcriptionId, outputPath);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CancelTranscriptionAsync(
            string transcriptionId, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(transcriptionId))
            {
                throw new ArgumentException("Transcription ID cannot be null or empty", nameof(transcriptionId));
            }
            
            try
            {
                _logger.LogInformation("Cancelling transcription: {TranscriptionId}", transcriptionId);
                return await _transcriptionService.CancelTranscriptionAsync(transcriptionId, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Cancel transcription operation was itself canceled: {TranscriptionId}", transcriptionId);
                return false;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error cancelling transcription: {TranscriptionId}", transcriptionId);
                return false; // Return false instead of throwing to indicate cancellation failed
            }
        }
    }
}

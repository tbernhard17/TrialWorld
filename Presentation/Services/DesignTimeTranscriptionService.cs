using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Transcription;

namespace TrialWorld.Presentation.Services
{
    /// <summary>
    /// Design-time implementation of ITranscriptionService for UI development and testing.
    /// This provides mock responses without requiring actual API calls or dependencies.
    /// </summary>
    public class DesignTimeTranscriptionService : ITranscriptionService
    {
        private readonly Random _random = new Random();
        
        /// <inheritdoc/>
        public Task<TranscriptionResult> TranscribeAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return TranscribeAsync(filePath, new TranscriptionConfig(), cancellationToken);
        }

        /// <inheritdoc/>
        public Task<TranscriptionResult> TranscribeAsync(string filePath, TranscriptionConfig config, CancellationToken cancellationToken = default)
        {
            var result = CreateMockTranscriptionResult(filePath);
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<TranscriptionResult> TranscribeWithProgressAsync(
            string filePath,
            IProgress<TranscriptionProgressUpdate> progress,
            CancellationToken cancellationToken)
        {
            return TranscribeWithProgressAsync(filePath, new TranscriptionConfig(), progress, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TranscriptionResult> TranscribeWithProgressAsync(
            string filePath,
            TranscriptionConfig config,
            IProgress<TranscriptionProgressUpdate> progress,
            CancellationToken cancellationToken)
        {
            // Simulate progress updates
            for (int i = 0; i <= 100; i += 10)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                
                TranscriptionPhase phase = i switch
                {
                    < 20 => TranscriptionPhase.SilenceDetection,
                    < 40 => TranscriptionPhase.AudioExtraction,
                    < 60 => TranscriptionPhase.Uploading,
                    < 80 => TranscriptionPhase.Processing,
                    < 100 => TranscriptionPhase.Downloading,
                    _ => TranscriptionPhase.Completed
                };
                
                progress?.Report(new TranscriptionProgressUpdate
                {
                    Phase = phase,
                    ProgressPercent = i,
                    FilePath = filePath,
                    TranscriptionId = $"mock-{Path.GetFileNameWithoutExtension(filePath)}"
                });
                
                await Task.Delay(100, cancellationToken);
            }
            
            return CreateMockTranscriptionResult(filePath);
        }

        /// <inheritdoc/>
        public Task<TranscriptionStatus> GetTranscriptionStatusAsync(string transcriptionId, CancellationToken cancellationToken = default)
        {
            // Always return completed for design-time
            return Task.FromResult(TranscriptionStatus.Completed);
        }

        /// <inheritdoc/>
        public Task<bool> CancelTranscriptionAsync(string transcriptionId, CancellationToken cancellationToken = default)
        {
            // Always succeed for design-time
            return Task.FromResult(true);
        }
        
        /// <inheritdoc/>
        public Task<bool> DownloadTranscriptionFileAsync(
            string transcriptionId, 
            string outputPath, 
            IProgress<TranscriptionProgressUpdate>? progress = null, 
            CancellationToken cancellationToken = default)
        {
            // Simulate progress
            progress?.Report(new TranscriptionProgressUpdate
            {
                Phase = TranscriptionPhase.Downloading,
                ProgressPercent = 50,
                FilePath = outputPath,
                TranscriptionId = transcriptionId
            });
            
            // Simulate a short delay
            Task.Delay(200, cancellationToken).Wait();
            
            // Report completion
            progress?.Report(new TranscriptionProgressUpdate
            {
                Phase = TranscriptionPhase.Completed,
                ProgressPercent = 100,
                FilePath = outputPath,
                TranscriptionId = transcriptionId
            });
            
            // Always succeed for design-time
            return Task.FromResult(true);
        }

        /// <summary>
        /// Creates a mock transcription result for design-time use
        /// </summary>
        private TranscriptionResult CreateMockTranscriptionResult(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string transcriptionId = $"mock-{fileName}-{DateTime.Now.Ticks}";
            
            // Create segments first with all required properties
            var segments = new List<TranscriptSegment>
            {
                new TranscriptSegment
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = $"This is the first segment for {fileName}.",
                    StartTime = 0,
                    EndTime = 5000,
                    Speaker = "Speaker 1",
                    Sentiment = "Neutral",
                    Confidence = 0.95
                },
                new TranscriptSegment
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = $"This is the second segment for {fileName}.",
                    StartTime = 5100,
                    EndTime = 10000,
                    Speaker = "Speaker 2",
                    Sentiment = "Positive",
                    Confidence = 0.92
                },
                new TranscriptSegment
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = $"This is the third segment for {fileName}.",
                    StartTime = 10100,
                    EndTime = 15000,
                    Speaker = "Speaker 1",
                    Sentiment = "Negative",
                    Confidence = 0.97
                }
            };
            
            // Create the result with all properties properly set
            var result = new TranscriptionResult
            {
                Id = transcriptionId,
                TranscriptId = transcriptionId,
                Status = TranscriptionStatus.Completed,
                AudioDuration = TimeSpan.FromSeconds(_random.Next(60, 3600)), // Convert seconds to TimeSpan
                Confidence = 0.95
            };
            
            // Set segments and build the full text from segments
            result.Segments = segments;
            
            // Note: We don't set Text directly as it's a read-only property
            // The actual implementation would calculate this from segments
            
            return result;
        }
    }
}

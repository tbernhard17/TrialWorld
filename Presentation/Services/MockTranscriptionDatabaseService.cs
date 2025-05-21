using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models.Transcription;
using TrialWorld.Core.Services;

namespace TrialWorld.Presentation.Services
{
    /// <summary>
    /// Mock implementation of ITranscriptionDatabaseService for design-time and testing
    /// </summary>
    public class MockTranscriptionDatabaseService : ITranscriptionDatabaseService
    {
        private readonly Dictionary<string, TranscriptionResult> _transcriptions = new Dictionary<string, TranscriptionResult>();
        
        /// <summary>
        /// Initializes a new instance of the MockTranscriptionDatabaseService class
        /// </summary>
        public MockTranscriptionDatabaseService()
        {
            // Add some mock data
            var mockTranscript = new TranscriptionResult
            {
                Id = "mock-transcript-1",
                Status = TranscriptionStatus.Completed,
                Transcript = "This is a mock transcript for testing purposes.",
                Language = "en",
                Confidence = 0.95,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };
            
            _transcriptions.Add(mockTranscript.Id, mockTranscript);
        }
        
        /// <summary>
        /// Gets a transcription by ID
        /// </summary>
        public Task<TranscriptionResult?> GetTranscriptionAsync(string id, CancellationToken cancellationToken = default)
        {
            if (_transcriptions.TryGetValue(id, out var transcription))
            {
                return Task.FromResult<TranscriptionResult?>(transcription);
            }
            
            return Task.FromResult<TranscriptionResult?>(null);
        }
        
        /// <summary>
        /// Gets all transcriptions
        /// </summary>
        public Task<IEnumerable<TranscriptionResult>> GetAllTranscriptionsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<TranscriptionResult>>(_transcriptions.Values);
        }
        
        /// <summary>
        /// Saves a transcription
        /// </summary>
        public Task<bool> SaveTranscriptionAsync(TranscriptionResult transcription, CancellationToken cancellationToken = default)
        {
            _transcriptions[transcription.Id] = transcription;
            return Task.FromResult(true);
        }
        
        /// <summary>
        /// Deletes a transcription
        /// </summary>
        public Task<bool> DeleteTranscriptionAsync(string id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_transcriptions.Remove(id));
        }
        
        /// <summary>
        /// Searches transcriptions
        /// </summary>
        public Task<IEnumerable<TranscriptionResult>> SearchTranscriptionsAsync(string query, CancellationToken cancellationToken = default)
        {
            var results = new List<TranscriptionResult>();
            
            foreach (var transcription in _transcriptions.Values)
            {
                if (transcription.Transcript != null && 
                    transcription.Transcript.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(transcription);
                }
            }
            
            return Task.FromResult<IEnumerable<TranscriptionResult>>(results);
        }
    }
}

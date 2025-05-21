using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Processing; // For the enum

namespace TrialWorld.Core.Interfaces.Persistence
{
    /// <summary>
    /// Interface for data access operations related to Media.
    /// Implementations will handle mapping between Core models and Persistence entities.
    /// </summary>
    public interface IMediaRepository
    {
        Task<MediaMetadata?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<List<MediaMetadata>> GetListAsync(string? filter = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<MediaMetadata> AddAsync(MediaMetadata mediaMetadata, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(MediaMetadata mediaMetadata, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

        // Methods specifically for related data
        Task<MediaTranscript?> GetTranscriptAsync(string mediaId, CancellationToken cancellationToken = default);
        Task<bool> SaveTranscriptAsync(MediaTranscript transcript, CancellationToken cancellationToken = default);

        // Method to get status (might return an entity or just the enum depending on implementation)
        // Let's return the enum directly from the repository to simplify the service layer
        Task<MediaProcessingStatus?> GetProcessingStatusAsync(string mediaId, CancellationToken cancellationToken = default);
        Task<bool> UpdateProcessingStatusAsync(string mediaId, MediaProcessingStatus status, CancellationToken cancellationToken = default);

        // Potentially other methods like FindByPathAsync, etc.
        Task<List<MediaMetadata>> GetByStatusAsync(MediaProcessingStatus status, CancellationToken cancellationToken = default);
    }
} 
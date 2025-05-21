using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Processing;
using TrialWorld.Persistence.Data;
using TrialWorld.Persistence.Entities;

namespace TrialWorld.Persistence.Repositories
{
    /// <summary>
    /// EF Core implementation of the media metadata repository.
    /// Adheres to Rule #2 (Implementations in Infrastructure/Persistence).
    /// Adheres to Rule #7 (Inject Logger).
    /// </summary>
    public class MediaMetadataRepository : IMediaMetadataRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MediaMetadataRepository> _logger;

        public MediaMetadataRepository(AppDbContext context, ILogger<MediaMetadataRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MediaInfo?> GetByIdAsync(Guid mediaId)
        {
            _logger.LogInformation("Fetching MediaMetadataEntity by Id: {MediaId}", mediaId);
            try
            {
                string mediaIdString = mediaId.ToString();
                var entity = await _context.MediaMetadataEntities
                                     // .Include(m => m.TranscriptSegments) // Example
                                     .FirstOrDefaultAsync(m => m.Id == mediaIdString);
                
                // TODO: Map entity to MediaInfo Core model
                return MapToCoreModel(entity); // Placeholder mapping
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching MediaMetadataEntity by Id: {MediaId}", mediaId);
                throw; // Re-throw to allow higher layers to handle (Rule #8)
            }
        }

        public async Task<IEnumerable<MediaInfo>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all MediaMetadataEntities");
            try
            {
                var entities = await _context.MediaMetadataEntities.ToListAsync();
                // TODO: Map entities to MediaInfo Core models
                return entities.Select(MapToCoreModel).OfType<MediaInfo>().ToList(); // Placeholder mapping
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all MediaMetadataEntities");
                throw;
            }
        }

        public async Task AddAsync(MediaInfo mediaInfo)
        {
             _logger.LogInformation("Adding new MediaMetadataEntity from MediaInfo with Id: {MediaId}", mediaInfo.Id);
            try
            {
                // TODO: Map MediaInfo Core model to MediaMetadataEntity
                var entity = MapFromCoreModel(mediaInfo); // Placeholder mapping
                await _context.MediaMetadataEntities.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) // More specific exception handling
            {
                _logger.LogError(ex, "Error adding MediaMetadataEntity with Id: {MediaId}", mediaInfo.Id);
                throw;
            }
        }

        public async Task UpdateAsync(MediaInfo mediaInfo)
        {
            _logger.LogInformation("Updating MediaMetadataEntity from MediaInfo with Id: {MediaId}", mediaInfo.Id);
            try
            {
                // TODO: Map MediaInfo Core model to MediaMetadataEntity
                var entity = MapFromCoreModel(mediaInfo); // Placeholder mapping
                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex) // Handle concurrency issues
            {
                _logger.LogError(ex, "Concurrency error updating MediaMetadataEntity with Id: {MediaId}", mediaInfo.Id);
                // Optionally implement retry logic or specific handling here
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating MediaMetadataEntity with Id: {MediaId}", mediaInfo.Id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid mediaId)
        {
            _logger.LogInformation("Deleting MediaMetadataEntity with Id: {MediaId}", mediaId);
            try
            {
                 string mediaIdString = mediaId.ToString();
                var entity = await _context.MediaMetadataEntities.FindAsync(mediaIdString);
                if (entity != null)
                {
                    _context.MediaMetadataEntities.Remove(entity);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning("Attempted to delete non-existent MediaMetadataEntity with Id: {MediaId}", mediaId);
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting MediaMetadataEntity with Id: {MediaId}", mediaId);
                throw;
            }
        }

        public async Task<MediaTranscript?> GetTranscriptAsync(string mediaId, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("GetTranscriptAsync is not fully implemented in MediaMetadataRepository.");
            await Task.CompletedTask;
            return null;
        }

        public async Task<bool> SaveTranscriptAsync(string mediaId, MediaTranscript transcript, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("SaveTranscriptAsync is not fully implemented in MediaMetadataRepository.");
            await Task.CompletedTask;
            return false;
        }

        public async Task<MediaProcessingStatus?> GetProcessingStatusAsync(string mediaId, CancellationToken cancellationToken = default)
        {
            var entity = await _context.MediaMetadataEntities
                .Where(m => m.Id == mediaId)
                .Select(m => new { m.Status }) // Select only the status from the entity
                .FirstOrDefaultAsync(cancellationToken);

             if (entity == null) return null;
             return entity.Status;
        }

        public async Task<bool> UpdateProcessingStatusAsync(string mediaId, MediaProcessingStatus status, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating processing status for MediaMetadataEntity {MediaId} to {Status}", mediaId, status);
            // Use ExecuteUpdate for efficiency if only updating status
             int updatedCount = await _context.MediaMetadataEntities
                .Where(m => m.Id == mediaId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(m => m.Status, status), cancellationToken);
             
             if (updatedCount == 0)
             {
                 _logger.LogWarning("UpdateProcessingStatusAsync: MediaMetadataEntity not found for ID {MediaId}", mediaId);
             }
            return updatedCount > 0;
        }

        // --- Placeholder Mapping Methods --- 
        // These need proper implementation, potentially using AutoMapper
        private MediaInfo? MapToCoreModel(MediaMetadataEntity? entity)
        {
            if (entity == null) return null;
            // Simplified mapping - does NOT include related data like transcripts
            return new MediaInfo 
            { 
                Id = entity.Id,
                FilePath = entity.FilePath, 
                Title = entity.Title,
                Duration = entity.Duration,
                LastModified = entity.ModifiedDate,
                CreationDate = entity.CreatedDate
                // Missing mappings for streams, metadata dictionary, etc.
            };
        }

        private MediaMetadataEntity MapFromCoreModel(MediaInfo model)
        {
            return new MediaMetadataEntity
            {
                Id = model.Id,
                Title = model.Title,
                FilePath = model.FilePath,
                Duration = model.Duration,
                CreatedDate = model.CreationDate ?? DateTime.UtcNow, // Handle null CreationDate
                ModifiedDate = model.LastModified,
                Status = MediaProcessingStatus.Unknown // Default status when creating/updating entity from core model
                // Missing mappings for FileSize, Width, Height, etc. 
            };
        }
        
        private MediaMetadataEntity MapToEntity(MediaMetadata model) // This method seems redundant/incorrectly named now?
        {
            // Map Core.Models.MediaMetadata to Persistence.Entities.MediaMetadataEntity
            return new MediaMetadataEntity 
            {
                Id = model.Id,
                Title = model.Title,
                FilePath = model.FilePath,
                Duration = model.Duration,
                CreatedDate = model.CreatedDate, 
                ModifiedDate = model.ModifiedDate, 
                FileSize = model.FileSize,
                // Status needs to be determined/set appropriately, not directly from MediaMetadata model
                Status = MediaProcessingStatus.Unknown 
                // Width/Height mapping if needed
            };
        }
    }
}
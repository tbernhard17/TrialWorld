using TrialWorld.Core.Models; // Assuming MediaInfo is here
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Repository interface for managing media metadata persistence.
    /// Adheres to Rule #2 (Interfaces in Core).
    /// </summary>
    public interface IMediaMetadataRepository
    {
        Task<MediaInfo?> GetByIdAsync(Guid mediaId);
        Task<IEnumerable<MediaInfo>> GetAllAsync();
        Task AddAsync(MediaInfo mediaInfo);
        Task UpdateAsync(MediaInfo mediaInfo);
        Task DeleteAsync(Guid mediaId);
        // Add other specific query methods as needed, e.g.:
        // Task<IEnumerable<MediaInfo>> FindByFilePathAsync(string filePath);
    }
}
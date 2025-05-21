using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Transcription;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Interface for loading media database files with flexible naming patterns.
    /// </summary>
    public interface IDatabaseLoaderService
    {
        /// <summary>
        /// Loads all media metadata from the database directory with flexible naming.
        /// </summary>
        /// <param name="namePattern">Optional custom file pattern (e.g., "oakley_media*.json")</param>
        Task<List<MediaMetadata>> LoadMediaDatabaseAsync(string? namePattern = null);
        
        /// <summary>
        /// Loads all keyword data from the database directory with flexible naming.
        /// </summary>
        /// <param name="namePattern">Optional custom file pattern (e.g., "oakley_keywords*.json")</param>
        Task<List<KeywordData>> LoadKeywordDatabaseAsync(string? namePattern = null);
        
        /// <summary>
        /// Loads all face data from the database directory with flexible naming.
        /// </summary>
        /// <param name="namePattern">Optional custom file pattern (e.g., "oakley_faces*.json")</param>
        Task<List<FaceData>> LoadFaceDatabaseAsync(string? namePattern = null);
        
        /// <summary>
        /// Gets a list of available transcript files with their metadata.
        /// </summary>
        Task<List<TranscriptFileInfo>> GetAvailableTranscriptsAsync();
        
        /// <summary>
        /// Loads a transcript file in any supported format (JSON, SRT, or TXT).
        /// </summary>
        /// <param name="filePath">Path to the transcript file</param>
        /// <returns>MediaTranscript object or null if loading failed</returns>
        Task<Transcript?> LoadTranscriptFileAsync(string filePath);
    }
}

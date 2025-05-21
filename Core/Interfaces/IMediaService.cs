using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Processing;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Service for managing media files and their metadata.
    /// </summary>
    public interface IMediaService
    {
        /// <summary>
        /// Gets a list of media items with optional filtering.
        /// </summary>
        /// <param name="contentType">Filter by content type (e.g., "video" or "audio").</param>
        /// <param name="page">Page number (1-based).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of media metadata.</returns>
        Task<List<TrialWorld.Core.Models.MediaMetadata>> GetMediaListAsync(
            string contentType = "",
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a media item by ID.
        /// </summary>
        /// <param name="id">Media ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Media metadata or null if not found.</returns>
        Task<TrialWorld.Core.Models.MediaMetadata?> GetMediaByIdAsync(
            string id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves a media file.
        /// </summary>
        /// <param name="mediaStream">Media content stream.</param>
        /// <param name="fileName">Original file name.</param>
        /// <param name="title">Media title.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result of the save operation.</returns>
        Task<MediaSaveResult> SaveMediaAsync(
            Stream mediaStream,
            string fileName,
            string title,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates media metadata.
        /// </summary>
        /// <param name="id">Media ID.</param>
        /// <param name="updateData">Update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> UpdateMediaAsync(
            string id,
            MediaUpdateData updateData,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a media item.
        /// </summary>
        /// <param name="id">Media ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> DeleteMediaAsync(
            string id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the transcript for a media item.
        /// </summary>
        /// <param name="id">Media ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Media transcript or null if not found.</returns>
        Task<MediaTranscript?> GetMediaTranscriptAsync(
            string id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves a transcript for a media item.
        /// </summary>
        /// <param name="id">Media ID.</param>
        /// <param name="transcript">Media transcript.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> SaveMediaTranscriptAsync(
            string id,
            MediaTranscript transcript,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the processing status for a media item.
        /// </summary>
        /// <param name="id">Media ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The processing status enum, or null if not found or not processing.</returns>
        Task<TrialWorld.Core.Models.Processing.MediaProcessingStatus?> GetProcessingStatusAsync(
            string id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Imports a media file from a local path.
        /// </summary>
        /// <param name="filePath">Path to the media file.</param>
        /// <param name="title">Title for the media.</param>
        /// <param name="description">Optional description.</param> 
        /// <param name="tags">Optional list of tags.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created media metadata.</returns>
        Task<MediaMetadata> ImportMediaAsync(
            string filePath,
            string title,
            string? description = null,
            List<string>? tags = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Initiates processing (transcription, analysis) for a media item.
        /// </summary>
        /// <param name="id">Media ID.</param>
        /// <param name="transcribe">Whether to perform transcription.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ProcessMediaAsync(
            string id,
            bool transcribe,
            CancellationToken cancellationToken = default);
    }
}
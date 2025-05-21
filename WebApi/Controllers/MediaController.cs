using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TrialWorld.Contracts;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Analysis;
using MediatR;

namespace TrialWorld.WebApi.Controllers
{
    /// <summary>
    /// Controller for media operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;
        private readonly ISearchService _searchService;
        // Enhancement service removed
        private readonly ILogger<MediaController> _logger;
        private readonly IMediator _mediator;
        private readonly IMediaProcessingService _mediaProcessingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaController"/> class
        /// </summary>
        /// <param name="mediaService">Media service</param>
        /// <param name="searchService">Search service</param>
        /// <param name="enhancementService">Enhancement service</param>
        /// <param name="logger">Logger</param>
        /// <param name="mediator">Mediator</param>
        /// <param name="mediaProcessingService">Media processing service</param>
        public MediaController(
            IMediaService mediaService,
            ISearchService searchService,

            ILogger<MediaController> logger,
            IMediator mediator,
            IMediaProcessingService mediaProcessingService)
        {
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
            // Enhancement service initialization removed
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mediaProcessingService = mediaProcessingService ?? throw new ArgumentNullException(nameof(mediaProcessingService));
        }

        /// <summary>
        /// Gets a list of media files with optional filtering
        /// </summary>
        /// <param name="contentType">Filter by content type</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of results per page</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of media items</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<MediaDto>), 200)]
        public async Task<IActionResult> GetMediaList(
            [FromQuery] string contentType = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Request received for media list, content type: {ContentType}, page: {Page}, pageSize: {PageSize}",
                    contentType, page, pageSize);

                var mediaList = await _mediaService.GetMediaListAsync(contentType, page, pageSize, cancellationToken);

                var result = new List<MediaDto>();
                foreach (var media in mediaList)
                {
                    result.Add(new MediaDto
                    {
                        Id = media.Id,
                        Title = media.Title,
                        ContentType = media.ContentType,
                        FileSize = media.FileSize,
                        Duration = media.Duration,
                        ThumbnailUrl = media.ThumbnailUrl,
                        MediaUrl = $"/api/media/{media.Id}/content",
                        CreatedDate = media.CreatedDate,
                        ModifiedDate = media.ModifiedDate,
                        IsTranscribed = media.IsTranscribed,
                        IsAnalyzed = false
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving media list");
                return StatusCode(500, "An error occurred while retrieving the media list");
            }
        }

        /// <summary>
        /// Gets a specific media item by ID
        /// </summary>
        /// <param name="id">Media ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Media item details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MediaDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMedia(
            string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Request received for media item: {Id}", id);

                var media = await _mediaService.GetMediaByIdAsync(id, cancellationToken);
                if (media == null)
                {
                    return NotFound($"Media with ID {id} not found");
                }

                var result = new MediaDto
                {
                    Id = media.Id,
                    Title = media.Title,
                    ContentType = media.ContentType,
                    FileSize = media.FileSize,
                    Duration = media.Duration,
                    ThumbnailUrl = media.ThumbnailUrl,
                    MediaUrl = $"/api/media/{media.Id}/content",
                    CreatedDate = media.CreatedDate,
                    ModifiedDate = media.ModifiedDate,
                    IsTranscribed = media.IsTranscribed,
                    IsAnalyzed = false,
                    Metadata = media.Metadata
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving media with ID: {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the media item");
            }
        }

        /// <summary>
        /// Gets the media content file
        /// </summary>
        /// <param name="id">Media ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Media file content</returns>
        [HttpGet("{id}/content")]
        [ProducesResponseType(typeof(FileStreamResult), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMediaContent(
            string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Request received for media content: {Id}", id);

                var media = await _mediaService.GetMediaByIdAsync(id, cancellationToken);
                if (media == null)
                {
                    return NotFound($"Media with ID {id} not found");
                }

                var contentPath = media.FilePath;
                if (!System.IO.File.Exists(contentPath))
                {
                    return NotFound($"Media file for ID {id} not found on disk");
                }

                var contentType = GetContentType(media.ContentType);
                var stream = new FileStream(contentPath, FileMode.Open, FileAccess.Read);

                return File(stream, contentType, Path.GetFileName(contentPath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving media content for ID: {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the media content");
            }
        }

        /// <summary>
        /// Gets the transcript for a media item
        /// </summary>
        /// <param name="id">Media ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Media transcript</returns>
        [HttpGet("{id}/transcript")]
        [ProducesResponseType(typeof(TrialWorld.Contracts.MediaTranscriptDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMediaTranscript(
            string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Request received for media transcript: {Id}", id);

                var transcript = await _mediaService.GetMediaTranscriptAsync(id, cancellationToken);
                if (transcript == null)
                {
                    return NotFound($"Transcript for media with ID {id} not found");
                }

                var result = new TrialWorld.Contracts.MediaTranscriptDto
                {
                    MediaId = id,
                    FullText = transcript.FullText,
                    Segments = transcript.Segments?.Select(s => new TrialWorld.Contracts.TranscriptSegmentDto
                    {
                        Id = s.Id,
                        MediaId = s.MediaId,
                        Text = s.Text,
                        StartTime = s.StartTime, // Already double
                        EndTime = s.EndTime,   // Already double
                        Speaker = s.Speaker,
                        Confidence = s.Confidence, // Already double
                        Sentiment = s.Sentiment,
                        Words = s.Words?.Select(w => new TrialWorld.Contracts.WordInfoDto {
                            Text = w.Text,
                            StartTime = w.StartTime,
                            EndTime = w.EndTime,
                            Confidence = w.Confidence
                        }).ToList()
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transcript for media ID: {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the media transcript");
            }
        }

        /// <summary>
        /// Uploads a new media file
        /// </summary>
        /// <param name="file">Media file</param>
        /// <param name="title">Title</param>
        /// <param name="description">Description</param>
        /// <param name="tags">Tags</param>
        /// <param name="autoProcess">Whether to automatically process the media</param>
        /// <param name="autoTranscribe">Whether to automatically transcribe the media</param>
        /// <param name="autoFaceAnalysis">Whether to automatically analyze faces</param>
        /// <param name="autoEmotionAnalysis">Whether to automatically analyze emotions</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Uploaded media metadata</returns>
        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MediaDto>> UploadMedia(
            [FromForm] IFormFile file,
            [FromForm] string title,
            [FromForm] string description = "",
            [FromForm] string tags = "",
            [FromForm] bool autoProcess = true,
            [FromForm] bool autoTranscribe = true,
            [FromForm] bool autoFaceAnalysis = true,
            [FromForm] bool autoEmotionAnalysis = true,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }

                var tagList = string.IsNullOrEmpty(tags)
                    ? new List<string>()
                    : tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim())
                        .ToList();

                // Save the file to a temporary location
                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(Path.GetTempPath(), fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                // Create media item
                var mediaItem = await _mediaService.ImportMediaAsync(
                    filePath,
                    title,
                    description,
                    tagList,
                    cancellationToken);

                // Process the media if requested
                if (autoProcess)
                {
                    await _mediaService.ProcessMediaAsync(
                        mediaItem.Id,
                        autoTranscribe,
                        cancellationToken);
                }

                return Ok(MapToDto(mediaItem));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading media");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while uploading media");
            }
        }

        /// <summary>
        /// Processes a media item
        /// </summary>
        /// <param name="id">Media ID</param>
        /// <param name="request">Processing request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Processing status</returns>
        [HttpPost("{id}/process")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MediaProcessingStatusDto>> ProcessMedia(
            string id,
            [FromBody] MediaProcessingRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var mediaItem = await _mediaService.GetMediaByIdAsync(id, cancellationToken);
                if (mediaItem == null)
                {
                    return NotFound($"Media with ID {id} not found");
                }

                // Start processing asynchronously
                var processingTask = _mediaService.ProcessMediaAsync(
                    id,
                    request.Transcribe,
                    cancellationToken);

                // Return a status immediately
                var status = new MediaProcessingStatusDto
                {
                    MediaId = id,
                    Status = "queued",
                    Progress = 0,
                    CurrentOperation = "Starting processing",
                    StartTime = new DateTime(DateTime.UtcNow.Ticks)
                };

                return Accepted(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing media with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while processing media with ID {id}");
            }
        }

        /// <summary>
        /// Gets the processing status of a media item
        /// </summary>
        /// <param name="id">Media ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Processing status</returns>
        [HttpGet("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MediaProcessingStatusDto>> GetProcessingStatus(
            string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var statusEnum = await _mediaService.GetProcessingStatusAsync(id, cancellationToken);
                if (!statusEnum.HasValue)
                {
                    // If status is null, it means no processing info was found
                    return NotFound($"No processing status found for media with ID {id}");
                }

                // Map the enum value to the DTO
                var statusDto = MapStatusEnumToDto(id, statusEnum.Value);

                return Ok(statusDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving processing status for media with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving processing status for media with ID {id}");
            }
        }

        /// <summary>
        /// Enhances a media item
        /// </summary>
        /// <param name="id">Media ID</param>
        /// <param name="request">Enhancement request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Path to the enhanced media file</returns>
        [HttpPost("{id}/enhance")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> EnhanceMedia(
            string id,
            [FromBody] MediaEnhancementRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var mediaItem = await _mediaService.GetMediaByIdAsync(id, cancellationToken);
                if (mediaItem == null)
                {
                    return NotFound($"Media with ID {id} not found");
                }

                // Enhancement feature removed. This endpoint is not implemented.
                return StatusCode(StatusCodes.Status501NotImplemented, "Media enhancement is not available in this build.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enhancing media with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while enhancing media with ID {id}: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a media item
        /// </summary>
        /// <param name="id">Media ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success or failure status</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteMedia(
            string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var mediaItem = await _mediaService.GetMediaByIdAsync(id, cancellationToken);
                if (mediaItem == null)
                {
                    return NotFound($"Media with ID {id} not found");
                }

                // Delete from media service
                await _mediaService.DeleteMediaAsync(id, cancellationToken);

                // Delete from search index using the correct method name
                await _searchService.RemoveFromIndexAsync(id, cancellationToken);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting media with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while deleting media with ID {id}");
            }
        }

        /// <summary>
        /// Updates media metadata
        /// </summary>
        /// <param name="id">Media ID</param>
        /// <param name="metadataDto">Media metadata</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated media metadata</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MediaDto>> UpdateMedia(
            string id,
            [FromBody] TrialWorld.Contracts.MediaMetadataDto metadataDto,
            CancellationToken cancellationToken = default)
        {
            if (metadataDto == null)
            {
                return BadRequest("Metadata cannot be null.");
            }

            try
            {
                _logger.LogInformation("Request received to update media item: {Id}", id);

                // Fetch existing media item
                var existingMedia = await _mediaService.GetMediaByIdAsync(id, cancellationToken);
                if (existingMedia == null)
                {
                    return NotFound($"Media with ID {id} not found");
                }

                // Prepare update data based ONLY on the DTO properties
                var updateData = new MediaUpdateData
                {
                    Title = metadataDto.Title, // Only update from DTO
                };

                // Call service to update
                // Assuming UpdateMediaAsync takes ID and MediaUpdateData
                var success = await _mediaService.UpdateMediaAsync(id, updateData, cancellationToken);

                if (!success)
                {
                    // Handle update failure (e.g., concurrency issue)
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update media metadata.");
                }

                // Fetch the updated media item to return the full DTO
                var updatedMedia = await _mediaService.GetMediaByIdAsync(id, cancellationToken);
                if (updatedMedia == null)
                {
                     // Should not happen if update succeeded, but handle defensively
                     return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve updated media metadata.");
                }

                // Map the updated Core model to the full MediaDto
                var resultDto = MapToDto(updatedMedia);
                return Ok(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating media with ID: {Id}", id);
                return StatusCode(500, "An error occurred while updating the media item");
            }
        }

        #region Helper Methods

        private TrialWorld.Contracts.MediaMetadataDto MapToMediaMetadataDto(TrialWorld.Core.Models.MediaMetadata mediaItem)
        {
            if (mediaItem == null)
                return new TrialWorld.Contracts.MediaMetadataDto();

            return new TrialWorld.Contracts.MediaMetadataDto
            {
                Title = mediaItem.Title ?? Path.GetFileNameWithoutExtension(mediaItem.FilePath) ?? string.Empty,
                // Description = mediaItem.Description // Property does not exist on MediaMetadata
            };
        }

        private TrialWorld.Core.Models.MediaMetadata MapToMediaMetadata(TrialWorld.Contracts.MediaMetadataDto dto)
        {
            if (dto == null)
                return new TrialWorld.Core.Models.MediaMetadata
                {
                    Title = string.Empty,
                    FilePath = string.Empty,
                    FileSize = 0,
                    Duration = TimeSpan.Zero,
                    ThumbnailUrl = string.Empty,
                    CreatedDate = DateTime.MinValue,
                    ModifiedDate = DateTime.MinValue,
                    IsTranscribed = false,
                    Metadata = new Dictionary<string, string>()
                };

            // Cannot map Description as it doesn't exist on the Core model
            return new TrialWorld.Core.Models.MediaMetadata
            {
                Title = dto.Title ?? string.Empty,
                // Assign other required properties to safe defaults if necessary
                FilePath = string.Empty,
                FileSize = 0,
                Duration = TimeSpan.Zero,
                ThumbnailUrl = string.Empty,
                CreatedDate = DateTime.MinValue,
                ModifiedDate = DateTime.MinValue,
                IsTranscribed = false,
                Metadata = new Dictionary<string, string>(),
            };
        }

        private MediaDto MapToDto(TrialWorld.Core.Models.MediaMetadata mediaItem)
        {
            return new MediaDto
            {
                Id = mediaItem.Id,
                Title = mediaItem.Title ?? Path.GetFileNameWithoutExtension(mediaItem.FilePath) ?? string.Empty,
                // Map MediaType enum to string ContentType
                ContentType = MapMediaTypeToContentTypeString(mediaItem.MediaType), // Use helper
                FileSize = mediaItem.FileSize,
                Duration = mediaItem.Duration,
                ThumbnailUrl = mediaItem.ThumbnailUrl, // Use ThumbnailUrl
                MediaUrl = $"/api/media/{mediaItem.Id}/content",
                CreatedDate = mediaItem.CreatedDate, // Use CreatedDate
                ModifiedDate = mediaItem.ModifiedDate, // Use ModifiedDate
                IsTranscribed = mediaItem.IsTranscribed,
                IsAnalyzed = false, // Use the general IsAnalyzed flag
                Metadata = mediaItem.Metadata?.ToDictionary(kv => kv.Key, kv => (string?)kv.Value) ?? new Dictionary<string, string?>() // Ensure nullable values
            };
        }

        private TrialWorld.Contracts.MediaTranscriptDto? MapToTranscriptDto(TrialWorld.Core.Models.Transcription.TranscriptionResult transcript)
        {
            if (transcript == null) return null;
            return new TrialWorld.Contracts.MediaTranscriptDto
            {
                MediaId = transcript.Id,
                FullText = transcript.Transcript,
                Segments = transcript.Segments?.Select(s => new TrialWorld.Contracts.TranscriptSegmentDto
                {
                    Id = s.Id,
                    MediaId = s.MediaId,
                    Text = s.Text,
                    StartTime = s.StartTime, // Already double
                    EndTime = s.EndTime,   // Already double
                    Speaker = s.Speaker,
                    Confidence = s.Confidence, // Already double
                    Sentiment = s.Sentiment,
                    Words = s.Words?.Select(w => new TrialWorld.Contracts.WordInfoDto {
                        Text = w.Text,
                        StartTime = w.StartTime,
                        EndTime = w.EndTime,
                        Confidence = w.Confidence
                    }).ToList()
                }).ToList() ?? new List<TrialWorld.Contracts.TranscriptSegmentDto>()
            };
        }

        // Corrected: Use MediaType enum as input
        private string MapMediaTypeToContentTypeString(TrialWorld.Core.Enums.MediaType mediaType) 
        {
            return mediaType switch
            {
                TrialWorld.Core.Enums.MediaType.Video => "video/mp4", // Example mapping
                TrialWorld.Core.Enums.MediaType.Audio => "audio/mpeg", // Example mapping
                TrialWorld.Core.Enums.MediaType.Image => "image/jpeg", // Example mapping
                _ => "application/octet-stream"
            };
        }

        // Helper to map float[] BoundingBox to RectangleDto
        private RectangleDto? MapBoundingBox(float[]? bbox)
        {
            if (bbox == null || bbox.Length < 4)
            {
                return null;
            }
            // Assuming order [x, y, width, height]
            return new RectangleDto
            {
                X = bbox[0],
                Y = bbox[1],
                Width = bbox[2],
                Height = bbox[3]
            };
        }

        private string GetContentType(string mediaType)
        {
            return mediaType?.ToLowerInvariant() switch
            {
                "video" => "video/mp4",
                "audio" => "audio/mpeg",
                _ => "application/octet-stream"
            };
        }

        // Helper method to map the status enum to the DTO
        private MediaProcessingStatusDto MapStatusEnumToDto(string mediaId, Core.Models.Processing.MediaProcessingStatus status)
        {
             return new MediaProcessingStatusDto
             {
                 MediaId = mediaId,
                 Status = status.ToString(), // Get string representation of enum
                 Progress = CalculateProgressFromStatus(status), // Use helper
                 CurrentOperation = GetOperationNameFromStatus(status), // Use helper
                 // Timestamps and StepStatus are not available from the basic enum
                 StartTime = null,
                 EndTime = null,
                 ErrorMessage = status == Core.Models.Processing.MediaProcessingStatus.Failed ? "Processing failed" : null,
                 StepStatus = null 
             };
        }

        // Placeholder helper to estimate progress
        private double CalculateProgressFromStatus(Core.Models.Processing.MediaProcessingStatus status)
        {
            return status switch
            {
                Core.Models.Processing.MediaProcessingStatus.Queued => 0,
                Core.Models.Processing.MediaProcessingStatus.Processing => 50, // Estimate
                Core.Models.Processing.MediaProcessingStatus.Completed => 100,
                Core.Models.Processing.MediaProcessingStatus.Failed => 100, // Or 0?
                Core.Models.Processing.MediaProcessingStatus.Cancelled => 0,
                _ => 0 // Default for Unknown/Paused etc.
            };
        }
        
        // Placeholder helper to get operation name
        private string GetOperationNameFromStatus(Core.Models.Processing.MediaProcessingStatus status)
        {
            return status switch
            {
                Core.Models.Processing.MediaProcessingStatus.Queued => "Waiting in queue",
                Core.Models.Processing.MediaProcessingStatus.Processing => "Processing media",
                Core.Models.Processing.MediaProcessingStatus.Completed => "Processing complete",
                Core.Models.Processing.MediaProcessingStatus.Failed => "Processing failed",
                Core.Models.Processing.MediaProcessingStatus.Paused => "Processing paused",
                Core.Models.Processing.MediaProcessingStatus.Cancelled => "Processing cancelled",
                _ => "Unknown status"
            };
        }

        #endregion
    }
}
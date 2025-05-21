using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TrialWorld.Contracts;
using TrialWorld.Core.Enums;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Transcription;
using TrialWorld.Core.Models.Processing;

namespace TrialWorld.WebApi.Controllers
{
    /// <summary>
    /// Controller for transcription operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TranscriptionController : ControllerBase
    {
        private readonly IMediaService _mediaService;
        private readonly ILogger<TranscriptionController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranscriptionController"/> class
        /// </summary>
        /// <param name="mediaService">Media service</param>
        /// <param name="logger">Logger</param>
        public TranscriptionController(
            IMediaService mediaService,
            ILogger<TranscriptionController> logger)
        {
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initiates a transcription request for a specific media file.
        /// </summary>
        /// <param name="request">The transcription request details.</param>
        /// <returns>Accepted response if the transcription job is successfully initiated.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTranscription([FromBody] TranscriptionRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.MediaId))
            {
                return BadRequest("Invalid request payload or missing Media ID.");
            }

            try
            {
                var media = await _mediaService.GetMediaByIdAsync(request.MediaId);
                if (media == null)
                {
                    _logger.LogWarning("CreateTranscription failed: Media not found for ID {MediaId}", request.MediaId);
                    return NotFound($"Media with ID '{request.MediaId}' not found.");
                }

                // Optional: Check if media type is suitable for transcription (e.g., audio/video)
                if (media.ContentType != "audio" && media.ContentType != "video")
                {
                     _logger.LogWarning("CreateTranscription attempted on unsupported media type {ContentType} for ID {MediaId}", media.ContentType, request.MediaId);
                     return BadRequest($"Transcription not supported for content type '{media.ContentType}'.");
                }

                // Optional: Check if already processing or transcribed
                var currentStatus = await _mediaService.GetProcessingStatusAsync(request.MediaId);
                if (currentStatus == MediaProcessingStatus.Processing ||
                    currentStatus == MediaProcessingStatus.Processing || // Assuming a specific Transcribing status exists
                    media.IsTranscribed) // Or check a specific flag if status isn't granular enough
                {
                    _logger.LogInformation("Transcription already processing or completed for Media ID {MediaId}", request.MediaId);
                    // Decide whether to return 202 anyway or a Conflict/Bad Request
                    // Returning 202 might be simpler for the client - idempotency
                    return Accepted(nameof(GetTranscriptionStatus), new { mediaId = request.MediaId });
                }

                // Initiate processing via MediaService
                // We are assuming ProcessMediaAsync handles calling the transcription service,
                // setting the status to Processing/Transcribing, and storing necessary job info.
                // We pass 'transcribe: true'. Add other flags if ProcessMediaAsync supports more (e.g., analysis)
                await _mediaService.ProcessMediaAsync(request.MediaId, transcribe: true, cancellationToken: HttpContext.RequestAborted);

                _logger.LogInformation("Transcription job initiated for Media ID {MediaId}", request.MediaId);

                // Return 202 Accepted with a link to the status endpoint
                return Accepted(nameof(GetTranscriptionStatus), new { mediaId = request.MediaId }); // Use mediaId for status check

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while initiating transcription for Media ID {MediaId}", request.MediaId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Gets the current status of a transcription job for a specific media file.
        /// </summary>
        /// <param name="mediaId">The ID of the media file.</param>
        /// <returns>The current transcription status.</returns>
        [HttpGet("status/{mediaId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTranscriptionStatus(string mediaId)
        {
             if (string.IsNullOrWhiteSpace(mediaId))
            {
                return BadRequest("Media ID cannot be empty.");
            }

            try
            {
                 // Check media exists first? Optional, depends on desired behavior if ID is invalid.
                 var media = await _mediaService.GetMediaByIdAsync(mediaId);
                 if (media == null)
                 {
                     _logger.LogWarning("GetTranscriptionStatus failed: Media not found for ID {MediaId}", mediaId);
                     return NotFound($"Media with ID '{mediaId}' not found.");
                 }

                var status = await _mediaService.GetProcessingStatusAsync(mediaId);

                if (!status.HasValue)
                {
                    // If media exists but has no processing status, assume it hasn't started or isn't applicable
                     _logger.LogInformation("No processing status found for Media ID {MediaId}", mediaId);
                    // Return status like "NotProcessed" or "Unknown" or even 404 depending on API contract
                    return Ok("Not Processed");
                }

                // Map Core enum to a string representation for the API response
                string statusString = MapProcessingStatusToString(status.Value);

                _logger.LogDebug("Returning status '{Status}' for Media ID {MediaId}", statusString, mediaId);
                return Ok(statusString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transcription status for Media ID {MediaId}", mediaId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Retrieves the completed transcription result for a specific media file.
        /// </summary>
        /// <param name="mediaId">The ID of the media file.</param>
        /// <returns>The transcription result.</returns>
        [HttpGet("{mediaId}")]
        [ProducesResponseType(typeof(Contracts.TranscriptionResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status202Accepted)] // Indicate still processing
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTranscription(string mediaId)
        {
            if (string.IsNullOrWhiteSpace(mediaId))
            {
                return BadRequest("Media ID cannot be empty.");
            }

            try
            {
                var media = await _mediaService.GetMediaByIdAsync(mediaId);
                 if (media == null)
                 {
                     _logger.LogWarning("GetTranscription failed: Media not found for ID {MediaId}", mediaId);
                     return NotFound($"Media with ID '{mediaId}' not found.");
                 }

                // Check status first
                var status = await _mediaService.GetProcessingStatusAsync(mediaId);

                 // Handle cases where transcription hasn't completed or failed
                if (status != MediaProcessingStatus.Completed)
                {
                    _logger.LogInformation("Transcription not yet complete for Media ID {MediaId}. Status: {Status}", mediaId, status);
                    string statusString = status.HasValue ? MapProcessingStatusToString(status.Value) : "Not Processed";
                     // Return 202 Accepted or another appropriate status code indicating it's not ready
                     // Include the status message for clarity
                    return Accepted(new { message = $"Transcription is not yet complete. Current status: {statusString}", mediaId = mediaId });
                }

                 // If completed, try to get the transcript
                 var transcript = await _mediaService.GetMediaTranscriptAsync(mediaId);

                if (transcript == null)
                {
                    // This might happen if status is Completed but transcript saving failed or wasn't triggered
                    _logger.LogWarning("Transcription status is Completed but transcript data not found for Media ID {MediaId}", mediaId);
                    // Decide response: Could be NotFound, or an Ok with empty/null data, or InternalServerError
                    return NotFound($"Transcript data not found for media ID '{mediaId}', although processing is marked complete.");
                }

                // Map the Core model to the Contract DTO
                // Create a new instance of Transcription.MediaTranscript from the Core.Models.MediaTranscript
                var transcriptionMediaTranscript = new TrialWorld.Core.Models.Transcription.MediaTranscript
                {
                    Id = transcript.Id,
                    FullText = transcript.FullText,
                    Language = transcript.Language ?? "en",
                    // Map other properties as needed
                };
                
                var resultDto = MapToTranscriptionResultDto(transcriptionMediaTranscript); // Use the existing helper

                _logger.LogInformation("Returning transcript for Media ID {MediaId}", mediaId);
                return Ok(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transcript for Media ID {MediaId}", mediaId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        // Helper method to map Core Processing Status to API string representation
        private string MapProcessingStatusToString(MediaProcessingStatus status)
        {
            return status switch
            {
                MediaProcessingStatus.Unknown => "Unknown",
                MediaProcessingStatus.Queued => "Queued",
                MediaProcessingStatus.Processing => "Processing",
                MediaProcessingStatus.Paused => "Paused",
                MediaProcessingStatus.Completed => "Completed",
                MediaProcessingStatus.Failed => "Failed",
                MediaProcessingStatus.Cancelled => "Cancelled",
                _ => "Unknown",
            };

        }

        // Keep the existing DTO mapping helper method
        private Contracts.TranscriptionResultDto? MapToTranscriptionResultDto(TrialWorld.Core.Models.Transcription.MediaTranscript? transcript)
        {
            if (transcript == null)
            {
                _logger.LogWarning("Attempted to map a null MediaTranscript to TranscriptionResultDto");
                return null;
            }

            return new Contracts.TranscriptionResultDto
            {
                Transcript = transcript.FullText,
                Speakers = null // Contracts.TranscriptionResultDto only has Transcript and Speakers, but MediaTranscript does not have Speakers; set to null or map if available
            };
        }
    }
}
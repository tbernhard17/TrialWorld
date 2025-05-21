using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Transcription;
using MediaTranscriptModel = TrialWorld.Core.Models.Transcription.MediaTranscript;
using TrialWorld.Persistence.Data;
using TrialWorld.Persistence.Entities;

namespace TrialWorld.Persistence.Repositories
{
    /// <summary>
    /// EF Core implementation of the media transcript repository.
    /// Adheres to Rule #2 (Implementations in Infrastructure/Persistence).
    /// Adheres to Rule #7 (Inject Logger).
    /// </summary>
    public class TranscriptRepository : ITranscriptRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TranscriptRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the TranscriptRepository class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger.</param>
        public TranscriptRepository(AppDbContext context, ILogger<TranscriptRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Helper Methods

        /// <summary>
        /// Maps a TranscriptSegmentEntity to a TranscriptSegment model.
        /// </summary>
        /// <param name="entity">The entity to map.</param>
        /// <returns>A mapped TranscriptSegment model.</returns>
        private static TranscriptSegment MapEntityToModel(TranscriptSegmentEntity entity)
        {
            return new TranscriptSegment
            {
                Id = entity.Id.ToString(),
                MediaId = entity.MediaId,
                StartTime = entity.StartTime.TotalMilliseconds,
                EndTime = entity.EndTime.TotalMilliseconds,
                Text = entity.Text ?? string.Empty,
                Confidence = entity.Confidence,
                Speaker = entity.SpeakerLabel ?? string.Empty,
                Sentiment = entity.SentimentScore?.ToString()
                // Words is not in entity, so it will be null
            };
        }

        /// <summary>
        /// Maps a TranscriptSegment model to a TranscriptSegmentEntity.
        /// </summary>
        /// <param name="model">The model to map.</param>
        /// <returns>A mapped TranscriptSegmentEntity.</returns>
        private static TranscriptSegmentEntity MapModelToEntity(TranscriptSegment model)
        {
            return new TranscriptSegmentEntity
            {
                Id = Guid.TryParse(model.Id, out var guid) ? guid : Guid.NewGuid(),
                MediaId = model.MediaId ?? string.Empty,
                StartTime = TimeSpan.FromMilliseconds(model.StartTime),
                EndTime = TimeSpan.FromMilliseconds(model.EndTime),
                Text = model.Text,
                Confidence = model.Confidence,
                SpeakerLabel = model.Speaker,
                SentimentScore = double.TryParse(model.Sentiment, out var score) ? score : (double?)null
            };
        }

        #endregion

        #region ITranscriptRepository Implementation

        /// <summary>
        /// Saves a transcript.
        /// </summary>
        /// <param name="transcript">The transcript to save.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task SaveTranscriptAsync(MediaTranscriptModel transcript, CancellationToken cancellationToken = default)
        {
            if (transcript == null)
                throw new ArgumentNullException(nameof(transcript));
            
            if (string.IsNullOrEmpty(transcript.Id))
                throw new ArgumentException("Transcript.Id cannot be null or empty", nameof(transcript));

            _logger.LogInformation("Saving transcript with ID: {TranscriptId}", transcript.Id);

            try
            {
                // Remove existing segments for this MediaId
                await DeleteByMediaIdAsync(transcript.Id, cancellationToken);

                // Add new segments
                if (transcript.Segments != null && transcript.Segments.Any())
                {
                    await AddSegmentsAsync(transcript.Segments, cancellationToken);
                    _logger.LogInformation("Successfully saved {SegmentCount} segments for transcript: {TranscriptId}", 
                        transcript.Segments.Count, transcript.Id);
                }
                else
                {
                    _logger.LogInformation("No segments to save for transcript: {TranscriptId}", transcript.Id);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error saving transcript: {TranscriptId}", transcript.Id);
                throw;
            }
        }

        /// <summary>
        /// Deletes a transcript by its ID.
        /// </summary>
        /// <param name="transcriptId">The transcript ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task DeleteTranscriptAsync(string transcriptId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(transcriptId))
                throw new ArgumentException("transcriptId cannot be null or empty", nameof(transcriptId));
            
            _logger.LogInformation("Deleting transcript: {TranscriptId}", transcriptId);
            
            try
            {
                await DeleteByMediaIdAsync(transcriptId, cancellationToken);
                _logger.LogInformation("Successfully deleted transcript: {TranscriptId}", transcriptId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error deleting transcript: {TranscriptId}", transcriptId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a transcript by its unique identifier.
        /// </summary>
        /// <param name="transcriptId">The transcript ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The transcript or null if not found.</returns>
        public async Task<MediaTranscriptModel?> GetTranscriptAsync(string transcriptId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(transcriptId))
            {
                _logger.LogWarning("Attempted to get transcript with null or empty transcriptId.");
                return null;
            }
            
            _logger.LogDebug("Fetching transcript entities for MediaId: {MediaId}", transcriptId);
            
            try
            {
                var segmentEntities = await _context.TranscriptSegmentEntities
                    .Where(s => s.MediaId == transcriptId)
                    .OrderBy(s => s.StartTime)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                if (!segmentEntities.Any())
                {
                    _logger.LogInformation("No transcript segments found for MediaId: {MediaId}", transcriptId);
                    return null;
                }

                var segments = segmentEntities.Select(MapEntityToModel).ToList();
                
                return new MediaTranscriptModel
                {
                    Id = transcriptId,
                    MediaId = transcriptId,
                    FullText = string.Join(" ", segments.Select(s => s.Text)),
                    Segments = segments,
                    Language = "en", // Default language
                    CreatedDate = DateTime.UtcNow // Placeholder, adjust if needed
                };
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error fetching transcript for MediaId: {MediaId}", transcriptId);
                throw;
            }
        }

        /// <summary>
        /// Adds transcript segments to the database.
        /// </summary>
        /// <param name="segments">The segments to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task AddSegmentsAsync(IEnumerable<TranscriptSegment> segments, CancellationToken cancellationToken = default)
        {
            if (segments == null)
                throw new ArgumentNullException(nameof(segments));

            try
            {
                var entities = segments.Select(MapModelToEntity).ToList();
                await _context.TranscriptSegmentEntities.AddRangeAsync(entities, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Added {Count} transcript segment entities to database", entities.Count);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error adding transcript segments.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a transcript by its media ID.
        /// </summary>
        /// <param name="mediaId">The media ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The transcript or null if not found.</returns>
        public async Task<MediaTranscriptModel?> GetByMediaIdAsync(string mediaId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(mediaId))
            {
                _logger.LogWarning("Attempted to get transcript with null or empty mediaId.");
                return null;
            }
            _logger.LogDebug("Fetching transcript entities for MediaId: {MediaId}", mediaId);
            try
            {
                var segmentEntities = await _context.TranscriptSegmentEntities
                    .Where(s => s.MediaId == mediaId)
                    .OrderBy(s => s.StartTime)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                if (!segmentEntities.Any()) return null;

                var coreSegments = segmentEntities.Select(entity => new TranscriptSegment {
                    Id = entity.Id.ToString(),
                    MediaId = entity.MediaId, // Add MediaId mapping
                    StartTime = entity.StartTime.TotalMilliseconds,
                    EndTime = entity.EndTime.TotalMilliseconds,
                    Text = entity.Text ?? string.Empty,
                    Confidence = entity.Confidence,
                    Speaker = entity.SpeakerLabel ?? string.Empty,
                    Sentiment = entity.SentimentScore?.ToString()
                    // Words is not in entity
                }).ToList();
                
                // NOTE: Language and CreatedDate are not available from DB; using placeholders.
                return new MediaTranscriptModel
                {
                    Id = mediaId, // This Id refers to the MediaTranscript's Id, which is the MediaId
                    MediaId = mediaId, // Explicitly set MediaId for the transcript
                    FullText = string.Join(" ", coreSegments.Select(s => s.Text)),
                    Segments = coreSegments, // coreSegments is already List<TrialWorld.Core.Models.Transcription.TranscriptSegment>
                    Language = "en", // Placeholder
                    CreatedDate = DateTime.UtcNow // Placeholder
                };
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error fetching transcript for MediaId: {MediaId}", mediaId);
                throw;
            }
        }

        /// <summary>
        /// Deletes transcript segments by media ID.
        /// </summary>
        /// <param name="mediaId">The media ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task DeleteByMediaIdAsync(string mediaId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(mediaId))
            {
                _logger.LogWarning("Attempted to delete transcript with null or empty mediaId.");
                return;
            }
            _logger.LogInformation("Deleting transcript segments for MediaId: {MediaId}", mediaId);
            try
            {
                var deletedCount = await _context.TranscriptSegmentEntities
                    .Where(s => s.MediaId == mediaId)
                    .ExecuteDeleteAsync(cancellationToken);
                
                if (deletedCount > 0)
                {
                    _logger.LogInformation("Deleted {Count} transcript segments for MediaId: {MediaId}", deletedCount, mediaId);
                }
                else
                {
                    _logger.LogInformation("No transcript segments found to delete for MediaId: {MediaId}", mediaId);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error deleting transcript segments for MediaId: {MediaId}", mediaId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves transcript segments by media ID.
        /// </summary>
        /// <param name="mediaId">The media ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A collection of transcript segments.</returns>
        public async Task<IEnumerable<TranscriptSegment>> GetByMediaIdAsync(Guid mediaId, CancellationToken cancellationToken = default)
        {
            string mediaIdString = mediaId.ToString();
            _logger.LogDebug("Fetching transcript segment entities for MediaId: {MediaId}", mediaIdString);
            try
            {
                var entities = await _context.TranscriptSegmentEntities
                    .Where(s => s.MediaId == mediaIdString)
                    .OrderBy(s => s.StartTime)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                if (!entities.Any())
                {
                    _logger.LogInformation("No transcript segment entities found for MediaId: {MediaId}", mediaIdString);
                    return new List<TranscriptSegment>();
                }
                // Map entities to core models
                return entities.Select(entity => new TranscriptSegment {
                    Id = entity.Id.ToString(),
                    MediaId = entity.MediaId,
                    StartTime = entity.StartTime.TotalMilliseconds,
                    EndTime = entity.EndTime.TotalMilliseconds,
                    Text = entity.Text,
                    Confidence = entity.Confidence,
                    Speaker = entity.SpeakerLabel,
                    Sentiment = entity.SentimentScore?.ToString()
                    // Words is not in entity
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transcript segment entities for MediaId: {MediaId}", mediaIdString);
                throw;
            }
        }

        /// <summary>
        /// Retrieves transcript segments by transcript ID.
        /// </summary>
        /// <param name="transcriptId">The transcript ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of transcript segments.</returns>
        public async Task<IList<TranscriptSegment>> GetSegmentsAsync(string transcriptId, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("GetSegmentsAsync using transcriptId is not implemented correctly. Assuming it means MediaId.");
            var entities = await _context.TranscriptSegmentEntities
                            .Where(s => s.MediaId == transcriptId)
                            .OrderBy(s => s.StartTime)
                            .ToListAsync(cancellationToken);
            // Map entities to core models
            return entities.Select(entity => new TranscriptSegment {
                Id = entity.Id.ToString(),
                MediaId = entity.MediaId,
                StartTime = entity.StartTime.TotalMilliseconds,
                EndTime = entity.EndTime.TotalMilliseconds,
                Text = entity.Text,
                Confidence = entity.Confidence,
                Speaker = entity.SpeakerLabel,
                Sentiment = entity.SentimentScore?.ToString()
                // Words is not in entity
            }).ToList();
        }

        /// <summary>
        /// Deletes a media transcript by its ID.
        /// </summary>
        /// <param name="transcriptId">The transcript ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public Task DeleteMediaTranscriptAsync(string transcriptId, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("DeleteMediaTranscriptAsync using transcriptId is not implemented correctly. Assuming it means delete by MediaId.");
            return DeleteByMediaIdAsync(transcriptId, cancellationToken);
        }

        /// <summary>
        /// Retrieves transcript segments by speaker and media ID.
        /// </summary>
        /// <param name="speakerLabel">The speaker label.</param>
        /// <param name="mediaId">The media ID.</param>
        /// <returns>A collection of transcript segments.</returns>
        public async Task<IEnumerable<TranscriptSegment>> GetSegmentsBySpeakerAsync(string speakerLabel, Guid mediaId)
        {
            string mediaIdString = mediaId.ToString();
            _logger.LogInformation("Fetching transcript segment entities for speaker '{SpeakerLabel}' for MediaId: {MediaId}", speakerLabel, mediaIdString);
            try
            {
                var entities = await _context.TranscriptSegmentEntities
                    .Where(s => s.SpeakerLabel == speakerLabel && s.MediaId == mediaIdString)
                    .OrderBy(s => s.StartTime)
                    .AsNoTracking()
                    .ToListAsync();

                if (!entities.Any())
                {
                    _logger.LogInformation("No transcript segments found for speaker '{SpeakerLabel}' and MediaId: {MediaId}", speakerLabel, mediaIdString);
                    return Enumerable.Empty<TranscriptSegment>();
                }
                return entities.Select(entity => new TranscriptSegment {
                    Id = entity.Id.ToString(),
                    MediaId = entity.MediaId,
                    StartTime = entity.StartTime.TotalMilliseconds,
                    EndTime = entity.EndTime.TotalMilliseconds,
                    Text = entity.Text,
                    Confidence = entity.Confidence,
                    Speaker = entity.SpeakerLabel,
                    Sentiment = entity.SentimentScore?.ToString()
                    // Words is not in entity
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve transcript segment entities for speaker '{SpeakerLabel}' for MediaId {MediaId}", speakerLabel, mediaIdString);
                throw;
            }
        }

        /// <summary>
        /// Retrieves transcript segments by text and media ID.
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <param name="mediaId">The media ID.</param>
        /// <returns>A collection of transcript segments.</returns>
        public async Task<IEnumerable<TranscriptSegment>> GetSegmentsByTextAsync(string searchText, Guid mediaId)
        {
            string mediaIdString = mediaId.ToString();
            _logger.LogInformation("Fetching transcript segment entities containing text '{SearchText}' for MediaId: {MediaId}", searchText, mediaIdString);
            try
            {
                var entities = await _context.TranscriptSegmentEntities
                    .Where(s => s.Text.Contains(searchText) && s.MediaId == mediaIdString)
                    .OrderBy(s => s.StartTime)
                    .AsNoTracking()
                    .ToListAsync();

                if (!entities.Any())
                {
                    _logger.LogInformation("No transcript segments found containing text '{SearchText}' for MediaId: {MediaId}", searchText, mediaIdString);
                    return Enumerable.Empty<TranscriptSegment>();
                }
                return entities.Select(entity => new TranscriptSegment {
                    Id = entity.Id.ToString(),
                    MediaId = entity.MediaId,
                    StartTime = entity.StartTime.TotalMilliseconds,
                    EndTime = entity.EndTime.TotalMilliseconds,
                    Text = entity.Text,
                    Confidence = entity.Confidence,
                    Speaker = entity.SpeakerLabel,
                    Sentiment = entity.SentimentScore?.ToString()
                    // Words is not in entity
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve transcript segment entities containing text '{SearchText}' for MediaId {MediaId}", searchText, mediaIdString);
                throw;
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Search;
using TrialWorld.Core.Models.Transcription;

namespace TrialWorld.Application.Services
{
    public class MediaIndexingService
    {
        private readonly IMediaMetadataRepository _metadataRepo;
        private readonly ITranscriptRepository _transcriptRepo;
        private readonly IAnalysisResultRepository<FaceData> _faceRepo;
        private readonly IAnalysisResultRepository<EmotionData> _emotionRepo;
        private readonly ISearchIndexer _indexer;
        private readonly ILogger<MediaIndexingService> _logger;

        public MediaIndexingService(
            IMediaMetadataRepository metadataRepo,
            ITranscriptRepository transcriptRepo,
            IAnalysisResultRepository<FaceData> faceRepo,
            IAnalysisResultRepository<EmotionData> emotionRepo,
            ISearchIndexer indexer,
            ILogger<MediaIndexingService> logger)
        {
            _metadataRepo = metadataRepo;
            _transcriptRepo = transcriptRepo;
            _faceRepo = faceRepo;
            _emotionRepo = emotionRepo;
            _indexer = indexer;
            _logger = logger;
        }

        public async Task<bool> ProcessAndIndexMediaAsync(Guid mediaId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Indexing media: {MediaId}", mediaId);

                var metadata = await _metadataRepo.GetByIdAsync(mediaId);
                var transcriptSegments = await _transcriptRepo.GetSegmentsAsync(mediaId.ToString(), cancellationToken);
                var transcript = await _transcriptRepo.GetByMediaIdAsync(mediaId.ToString(), cancellationToken);
                var faces = await _faceRepo.GetByMediaIdAsync(mediaId);
                var emotions = await _emotionRepo.GetByMediaIdAsync(mediaId);

                if (metadata == null)
                {
                    _logger.LogWarning("No metadata found for media: {MediaId}", mediaId);
                    return false;
                }

                var content = new SearchableContent
                {
                    Id = mediaId.ToString(),
                    MediaId = mediaId.ToString(),
                    Title = metadata.Title ?? string.Empty,
                    Description = metadata.Metadata?.TryGetValue("Description", out var desc) ?? false ? desc : string.Empty,
                    Transcript = transcript?.FullText ?? (transcriptSegments != null ? string.Join(" ", transcriptSegments.Select(s => s.Text)) : string.Empty),
                    Tags = metadata?.Metadata?.Keys.ToList() ?? new List<string>(),
                    CreatedAt = metadata?.CreationDate ?? DateTime.UtcNow,
                    Duration = metadata?.Duration ?? TimeSpan.Zero, // Handle potential null metadata
                    Timestamps = transcript?.Segments != null 
                        ? transcript.Segments.Select(s => new SearchableTimestamp
                        {
                            Text = s?.Text ?? string.Empty,
                            Start = TimeSpan.FromMilliseconds(s?.StartTime ?? 0),
                            End = TimeSpan.FromMilliseconds(s?.EndTime ?? 0),
                            Confidence = (float)(s?.Confidence ?? 0)
                        }).ToList() 
                        : new List<SearchableTimestamp>(),
                    Metadata = metadata.Metadata ?? new Dictionary<string, string>()
                };

                if (metadata?.FilePath != null && !string.IsNullOrEmpty(metadata.FilePath) && content.Metadata != null)
                {
                    content.Metadata["OriginalFilePath"] = metadata.FilePath;
                }

                await _indexer.IndexAsync(mediaId.ToString(), content, cancellationToken);
                _logger.LogInformation("Successfully indexed media: {MediaId}", mediaId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to index media: {MediaId}", mediaId);
                return false;
            }
        }
    }
}

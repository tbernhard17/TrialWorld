using AutoMapper;
using TrialWorld.Contracts;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Transcription;
using MediaTranscriptModel = TrialWorld.Core.Models.Transcription.MediaTranscript;
// Uncomment if needed:
// using TrialWorld.Application.Models;
// using TrialWorld.Core.Models.Media;

namespace TrialWorld.Application.MappingProfiles
{
    /// <summary>
    /// AutoMapper profile that defines mappings between DTOs and domain models
    /// </summary>
    public class DtoMappingProfile : Profile
    {
        public DtoMappingProfile()
        {
            // Media mappings - Assuming DTOs exist in Application.Models
            CreateMap<MediaMetadataDto, MediaMetadata>().ReverseMap();
            CreateMap<MediaTranscriptDto, MediaTranscriptModel>().ReverseMap();
            CreateMap<TrialWorld.Application.Models.TranscriptSegmentDto, TranscriptSegment>().ReverseMap();
            CreateMap<TrialWorld.Application.Models.WordInfoDto, WordInfo>().ReverseMap(); // Added mapping for WordInfo
            
            // Processing mappings
            // CreateMap<MediaProcessingRequestDto, ProcessingOptions>().ReverseMap(); // ProcessingOptions is in TrialWorld.Core.Models
            
            // Enhancement mappings
            // CreateMap<MediaEnhancementRequestDto, VideoEnhancementOptions>().ReverseMap(); // VideoEnhancementOptions is in TrialWorld.Core.Models
            // CreateMap<MediaEnhancementRequestDto, EnhancementConfig>().ReverseMap(); // EnhancementConfig needs location check
            
            // Example: Map Core Model to DTO
            // CreateMap<MediaMetadata, MediaMetadataDto>(); // Already done above with ReverseMap
            // CreateMap<MediaTranscript, MediaTranscriptDto>(); // Already done above with ReverseMap
        }
    }
}

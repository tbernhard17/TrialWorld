using TrialWorld.Core.Models;
using TrialWorld.Application.Models;

namespace TrialWorld.Core.Models
{
    public static class UserPreferencesDtoExtensions
    {
        public static UserPreferences ToCore(this UserPreferencesDto dto)
        {
            return new UserPreferences
            {
                FavoriteTags = dto.FavoriteTags ?? new List<string>(),
                EmotionWeights = dto.EmotionWeights ?? new Dictionary<string, double>(),
                // Map other fields as needed
                // TopicWeights, SpeakerWeights, CustomWeights, etc. can be mapped if present in dto
            };
        }

        public static UserPreferences ToCoreModel(this UserPreferencesDto dto)
        {
            if (dto == null) return new UserPreferences();

            return new UserPreferences
            {
                // Map properties carefully
                // Example: Assuming UserPreferences (Core model) has similar properties
                // FavoriteTags = dto.FavoriteTags ?? new List<string>(),
                // RecentQueries = dto.RecentQueries ?? new List<string>()
                // UserId needs to be sourced elsewhere
            };
        }
    }
}

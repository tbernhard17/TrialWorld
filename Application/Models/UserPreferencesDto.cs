using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrialWorld.Application.Models
{
    /// <summary>
    /// Represents user preferences for search personalization
    /// </summary>
    public class UserPreferencesDto
    {
        [JsonPropertyName("favoriteTags")]
        public List<string>? FavoriteTags { get; set; }

        [JsonPropertyName("recentQueries")]
        public List<string>? RecentQueries { get; set; }

        [JsonPropertyName("emotionWeights")]
        public Dictionary<string, double>? EmotionWeights { get; set; }

        [JsonPropertyName("lastQuery")]
        public string? LastQuery { get; set; }

        [JsonPropertyName("lastRelevanceScore")]
        public float LastRelevanceScore { get; set; }

        [JsonPropertyName("preferredContentTypes")]
        public List<string>? PreferredContentTypes { get; set; }

        [JsonPropertyName("maxDuration")]
        public int? MaxDuration { get; set; }
    }
}

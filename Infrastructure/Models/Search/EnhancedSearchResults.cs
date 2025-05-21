using System.Collections.Generic;

namespace TrialWorld.Infrastructure.Models.Search
{
    public class EnhancedSearchResults<T>
    {
        public List<EnhancedSearchResult> Items { get; set; } = new();
        public int TotalMatches { get; set; }
        public double SearchTimeMs { get; set; }
    }

    public class EnhancedSearchResult
    {
        public string MediaPath { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public System.TimeSpan Timestamp { get; set; }
        public string? TranscriptSnippet { get; set; }
        public List<string> ThumbnailPaths { get; set; } = new();
    }
}

using System.Collections.Generic;

namespace TrialWorld.Infrastructure.Models.Search
{
    public class EnhancedSearchOptions
    {
        public string Query { get; set; } = string.Empty;
        public bool IncludeTranscripts { get; set; }
        public int MaxResults { get; set; } = 10;
    }
}

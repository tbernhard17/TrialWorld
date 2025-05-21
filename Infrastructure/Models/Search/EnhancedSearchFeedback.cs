namespace TrialWorld.Infrastructure.Models.Search
{
    public class EnhancedSearchFeedback
    {
        public string MediaId { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public double UserScore { get; set; }
    }
}

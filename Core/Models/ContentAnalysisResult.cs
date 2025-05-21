namespace TrialWorld.Core.Models
{
    public class ContentAnalysisResult
    {
        public string? Summary { get; set; }
        public List<string>? Topics { get; set; }
        public List<ContentHighlight>? Highlights { get; set; }
        public List<ThumbnailSuggestion>? Thumbnails { get; set; }
    }
}

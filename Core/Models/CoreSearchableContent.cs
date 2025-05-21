namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Represents content that can be indexed and searched.
    /// </summary>
    public class CoreSearchableContent
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string[] Tags { get; set; } = Array.Empty<string>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
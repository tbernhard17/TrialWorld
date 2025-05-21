namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Represents application-specific searchable content.
    /// </summary>
    public class AppSearchableContent
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Metadata { get; set; } = string.Empty;
    }
}
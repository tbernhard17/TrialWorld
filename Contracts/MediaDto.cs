namespace TrialWorld.Contracts
{
    public class MediaDto
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? ContentType { get; set; }
        public long? FileSize { get; set; }
        public TimeSpan? Duration { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? MediaUrl { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsTranscribed { get; set; }
        public bool IsAnalyzed { get; set; }
        public Dictionary<string, string?>? Metadata { get; set; }
    }
}
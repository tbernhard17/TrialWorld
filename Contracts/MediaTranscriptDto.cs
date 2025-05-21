namespace TrialWorld.Contracts
{
    public class MediaTranscriptDto
    {
        public string? MediaId { get; set; }
        public string? FullText { get; set; }
        public List<TranscriptSegmentDto>? Segments { get; set; }
    }
}
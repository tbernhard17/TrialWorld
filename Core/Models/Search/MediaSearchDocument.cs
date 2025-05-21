using TrialWorld.Core.Models.Transcription;
namespace TrialWorld.Core.Models.Search
{
    /// <summary>
    /// Represents a document for media search indexing.
    /// </summary>
    public class MediaSearchDocument
    {
        public Guid MediaId { get; set; }
        public string? Title { get; set; }
        public string? Transcript { get; set; }
        public List<string>? Tags { get; set; }
        public double? RelevanceScore { get; set; }
        public DateTime IndexedAt { get; set; }
        public List<TranscriptSegment>? Segments { get; set; }
        public DateTime Created { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
        public List<SpeakerSummary> Speakers { get; set; } = new();
    }
}

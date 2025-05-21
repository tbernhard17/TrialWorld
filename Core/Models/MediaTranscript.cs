using System;
using System.Collections.Generic;
using TrialWorld.Core.Models.Transcription; // Added for the consolidated TranscriptSegment

namespace TrialWorld.Core.Models
{
    public class MediaTranscript
    {
        public string Id { get; set; } = string.Empty;
        public string? MediaId { get; set; }
        public string? FullText { get; set; }
        public string? Language { get; set; }
        public double Confidence { get; set; }
        public List<TranscriptSegment> Segments { get; set; } = new List<TranscriptSegment>();
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? FilePath { get; set; }
        public TimeSpan Duration { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    // Removed TranscriptSegment definition from here

    public class Speaker
    {
        public string Label { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}

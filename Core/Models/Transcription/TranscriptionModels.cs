using System;
using System.Collections.Generic;

namespace TrialWorld.Core.Models.Transcription
{
    public class Transcript
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<TranscriptSegment> Segments { get; set; } = new List<TranscriptSegment>();
    }

    // Removed TranscriptSegment definition from here

    // public class Speaker { ... } // REMOVED

    // public class Sentiment { ... } // REMOVED
}

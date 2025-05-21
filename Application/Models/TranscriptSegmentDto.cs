using System;
using System.Collections.Generic;

namespace TrialWorld.Application.Models
{
    // Added WordInfoDto to mirror Core.Models.Transcription.WordInfo
    public class WordInfoDto
    {
        public string Text { get; set; } = string.Empty;
        public double StartTime { get; set; } // In milliseconds
        public double EndTime { get; set; }   // In milliseconds
        public double Confidence { get; set; }
    }

    /// <summary>
    /// Data transfer object for a transcript segment, used for mapping and API responses.
    /// </summary>
    public class TranscriptSegmentDto
    {
        public string? Id { get; set; } // Changed to nullable to match core model
        public string? MediaId { get; set; }
        public string? Text { get; set; }
        public double Confidence { get; set; }
        public string? Speaker { get; set; } // Renamed from SpeakerLabel
        public double StartTime { get; set; } // Changed from TimeSpan to double (milliseconds)
        public double EndTime { get; set; }   // Changed from TimeSpan to double (milliseconds)
        public string? Sentiment { get; set; } // Added to match core model
        public List<WordInfoDto>? Words { get; set; } // Added to match core model
        public Dictionary<string, string>? Metadata { get; set; } // Kept as it might be used
    }
}

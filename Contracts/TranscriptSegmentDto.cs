using System;
using System.Collections.Generic; // Required for List

namespace TrialWorld.Contracts
{
    // Added WordInfoDto to mirror Core.Models.Transcription.WordInfo for contracts
    public class WordInfoDto
    {
        public string Text { get; set; } = string.Empty;
        public double StartTime { get; set; } // In milliseconds
        public double EndTime { get; set; }   // In milliseconds
        public double Confidence { get; set; }
    }

    public class TranscriptSegmentDto
    {
        public string? Id { get; set; } // Added Id
        public string? MediaId { get; set; } // Added MediaId
        public string? Text { get; set; }
        public double StartTime { get; set; } // Changed from TimeSpan to double
        public double EndTime { get; set; }   // Changed from TimeSpan to double
        public string? Speaker { get; set; } // Renamed from SpeakerLabel
        public double Confidence { get; set; } // Changed from float to double
        public string? Sentiment { get; set; } // Added Sentiment
        public List<WordInfoDto>? Words { get; set; } // Added Words
    }
}
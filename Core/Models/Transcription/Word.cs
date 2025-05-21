using System;

namespace TrialWorld.Core.Models.Transcription
{
    /// <summary>
    /// Represents a word in a transcription.
    /// </summary>
    public class Word
    {
        public double Start { get; set; }
        public double End { get; set; }
        public string Text { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string? SpeakerId { get; set; }
    }
}

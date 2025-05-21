namespace TrialWorld.Core.Models.Transcription
{
    public class SpeakerSegment
    {
        public string SpeakerId { get; set; } = string.Empty;
        public string? Text { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }
    }
}

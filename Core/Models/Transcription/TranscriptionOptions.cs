using System.Collections.Generic;

namespace TrialWorld.Core.Models.Transcription
{
    public class TranscriptionOptions
    {
        public string Language { get; set; } = "en";
        public bool EnableSpeakerDiarization { get; set; } = true;
        public int MaxSpeakers { get; set; } = 10;
        public bool EnableWordTimestamps { get; set; } = true;
        public bool EnablePunctuation { get; set; } = true;
        public bool FilterProfanity { get; set; } = false;
        public double MinimumConfidence { get; set; } = 0.7;
        public Dictionary<string, object> CustomSettings { get; set; } = new();
    }
}
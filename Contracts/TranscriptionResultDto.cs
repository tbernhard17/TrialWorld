namespace TrialWorld.Contracts
{
    public class TranscriptionResultDto
    {
        public string? Transcript { get; set; }
        public List<SpeakerDto>? Speakers { get; set; }
    }
}
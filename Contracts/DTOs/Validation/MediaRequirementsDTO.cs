namespace TrialWorld.Contracts.DTOs.MediaValidation;

public class MediaRequirementsDTO
{
    public int MaxFileSizeMb { get; set; }
    public TimeSpan MinDuration { get; set; }
    public TimeSpan MaxDuration { get; set; }
    public bool RequiresVideo { get; set; }
    public bool RequiresAudio { get; set; }

    public AudioRequirementsDTO AudioRequirements { get; set; } = new AudioRequirementsDTO
    {
        MinimumSampleRateHz = 0,
        MinimumBitrateKbps = 0,
        MinimumLoudnessDb = 0.0,
        MaximumNoiseLevelDb = 0.0,
        RequireVocalPresence = false
    };
    public VideoRequirementsDTO VideoRequirements { get; set; } = new VideoRequirementsDTO
    {
        MinimumResolutionWidth = 0,
        MinimumResolutionHeight = 0,
        MinimumFramerate = 0.0,
        MinimumBitrateKbps = 0,
        RequireColor = false,
        RequireStableImage = false
    };
}

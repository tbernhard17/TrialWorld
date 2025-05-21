namespace TrialWorld.Contracts.DTOs.MediaValidation;

public class AudioRequirementsDTO
{
    // Old properties (for backward compatibility)
    public int MinSampleRate { get; set; }
    public int MaxSampleRate { get; set; }
    public int MinBitrate { get; set; }
    public int MaxBitrate { get; set; }
    public bool RequireMono { get; set; }
    public bool RequireStereo { get; set; }

    // New required properties
    public required int MinimumSampleRateHz { get; set; }
    public required int MinimumBitrateKbps { get; set; }
    public required double MinimumLoudnessDb { get; set; }
    public required double MaximumNoiseLevelDb { get; set; }
    public required bool RequireVocalPresence { get; set; }
}

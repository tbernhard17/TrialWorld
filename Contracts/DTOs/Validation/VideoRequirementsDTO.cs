namespace TrialWorld.Contracts.DTOs.MediaValidation;

public class VideoRequirementsDTO
{
    // Old properties (for backward compatibility)
    public int MinWidth { get; set; }
    public int MinHeight { get; set; }
    public int MaxWidth { get; set; }
    public int MaxHeight { get; set; }
    public int MinBitrate { get; set; }
    public int MaxBitrate { get; set; }
    public double MinFrameRate { get; set; }
    public double MaxFrameRate { get; set; }

    // New required properties
    public required int MinimumResolutionWidth { get; set; }
    public required int MinimumResolutionHeight { get; set; }
    public required double MinimumFramerate { get; set; }
    public required int MinimumBitrateKbps { get; set; }
    public required bool RequireColor { get; set; }
    public required bool RequireStableImage { get; set; }
}

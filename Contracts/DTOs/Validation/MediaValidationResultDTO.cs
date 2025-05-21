namespace TrialWorld.Contracts.DTOs.MediaValidation;

public class MediaValidationResultDTO
{
    public bool IsValid { get; set; }
    public List<ValidationErrorDTO> Errors { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

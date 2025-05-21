namespace TrialWorld.Contracts.DTOs.MediaValidation;

public class MediaValidationRequestDTO
{
    public string FilePath { get; set; } = string.Empty;
    public MediaRequirementsDTO Requirements { get; set; } = new();
}

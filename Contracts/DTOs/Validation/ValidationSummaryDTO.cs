namespace TrialWorld.Contracts.DTOs.MediaValidation;

public class ValidationSummaryDTO
{
    public string FilePath { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public List<ValidationErrorDTO> Errors { get; set; } = new();
}

namespace TrialWorld.Contracts.DTOs.MediaValidation;

public class ValidationErrorDTO
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "Error"; // or Warning
    public string Code { get; set; } = string.Empty;

    public ValidationErrorDTO() { }

    public ValidationErrorDTO(string field, string message)
    {
        Field = field;
        Message = message;
    }
}

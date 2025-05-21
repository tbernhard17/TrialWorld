using System.Text.Json.Serialization;

namespace TrialWorld.AssemblyAIDiagnostic.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object for AssemblyAI upload responses.
    /// </summary>
    public class UploadResponseDto
    {
        /// <summary>
        /// The URL of the uploaded audio file.
        /// </summary>
        [JsonPropertyName("upload_url")]
        public string UploadUrl { get; set; } = string.Empty;
    }
}

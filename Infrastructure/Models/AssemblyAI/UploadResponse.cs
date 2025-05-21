using System.Text.Json.Serialization;

namespace TrialWorld.Infrastructure.Models.AssemblyAI
{
    /// <summary>
    /// Placeholder for AssemblyAI upload response.
    /// </summary>
    public class UploadResponse
    {
        [JsonPropertyName("upload_url")]
        public string? UploadUrl { get; set; }
        // Add other potential fields if needed based on API docs
    }

    // Alias for consistency if used elsewhere
    public class AssemblyAIUploadResponse : UploadResponse { }
} 
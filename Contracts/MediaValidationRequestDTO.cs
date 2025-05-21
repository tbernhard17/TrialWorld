using System;
using System.Collections.Generic;

namespace TrialWorld.Contracts
{
    /// <summary>
    /// Data transfer object for requesting media validation against requirements
    /// </summary>
    public class MediaValidationRequestDTO
    {
        /// <summary>
        /// ID of the media file to validate
        /// </summary>
        public string MediaId { get; set; } = string.Empty;
        
        /// <summary>
        /// Path to the media file
        /// </summary>
        public string FilePath { get; set; } = string.Empty;
        
        /// <summary>
        /// Video requirements for validation
        /// </summary>
        public VideoRequirementsDTO VideoRequirements { get; set; } = VideoRequirementsDTO.CreateDefault();
        
        /// <summary>
        /// Audio requirements for validation
        /// </summary>
        public AudioRequirementsDTO AudioRequirements { get; set; } = AudioRequirementsDTO.CreateDefault();
        
        /// <summary>
        /// Whether to generate a detailed report of the validation
        /// </summary>
        public bool GenerateDetailedReport { get; set; } = true;
        
        /// <summary>
        /// Tags to associate with this validation request
        /// </summary>
        public List<string>? Tags { get; set; } = new();
    }
}

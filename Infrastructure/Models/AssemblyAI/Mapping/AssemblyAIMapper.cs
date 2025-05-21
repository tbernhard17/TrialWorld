using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Transcription;
using TrialWorld.Infrastructure.Models.AssemblyAI.DTOs;
using TrialWorld.Infrastructure.Transcription.DTOs;

namespace TrialWorld.Infrastructure.Models.AssemblyAI.Mapping
{
    /// <summary>
    /// Provides mapping functionality between AssemblyAI API models and Core domain models.
    /// This class is intended for backward compatibility with the legacy direct API implementation.
    /// New implementations should use the SDK directly.
    /// </summary>
    [Obsolete("This legacy mapper is deprecated. Use direct SDK mapping in the service implementations.")]
    public static class AssemblyAIMapper
    {
        /// <summary>
        /// Maps a TranscriptionConfig to an AssemblyAI TranscriptionRequest.
        /// </summary>
        /// <param name="config">The core transcription configuration.</param>
        /// <param name="audioUrl">The URL of the uploaded audio file.</param>
        /// <returns>A mapped TranscriptionRequest.</returns>
        public static TranscriptionRequestDto MapToTranscriptionRequest(TranscriptionConfig config, string audioUrl)
        {
            // Map config fields to DTO fields. Ensure only API-required fields are present in DTO.
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (string.IsNullOrEmpty(audioUrl)) throw new ArgumentException("Audio URL cannot be empty", nameof(audioUrl));
            
            return new TranscriptionRequestDto
            {
                AudioUrl = audioUrl,
                SpeakerLabels = false, // Speaker diarization OFF
                SentimentAnalysis = config.EnableSentimentAnalysis, // Only sentiment ON
                LanguageCode = string.IsNullOrEmpty(config.LanguageCode) ? "en" : config.LanguageCode,
                // Add standard properties from the TranscriptionRequestDto
                Punctuate = false,     // Punctuation OFF
                FormatText = false,    // Text formatting OFF
                WebhookUrl = config.WebhookUrl,
                WebhookAuthHeaderName = config.WebhookAuthHeaderName,
                WebhookAuthHeaderValue = config.WebhookAuthHeaderValue
            };
        }
        
        /// <summary>
        /// OBSOLETE: Use AssemblyAITranscriptionMapper.ToCore for all DTO-to-Core mapping.
        /// </summary>
        /// <remarks>
        /// This method is obsolete. Use AssemblyAITranscriptionMapper.ToCore(AssemblyAITranscriptionResponseDto) instead.
        /// </remarks>
        public static TranscriptionResult MapToTranscriptionResult(AssemblyAITranscriptionResponseDto response)
        {
            // Delegate to the new centralized mapper
            return AssemblyAITranscriptionMapper.ToCore(response);
        }
        
        /// <summary>
        /// Maps a core transcription model name to an AssemblyAI model string.
        /// </summary>
        private static string MapTranscriptionModel(string? modelName)
        {
            if (string.IsNullOrEmpty(modelName))
                return "nova"; // Default to Nova as it's the fastest
                
            return modelName.ToLowerInvariant() switch
            {
                "aai.speechmodel.nano" => "nova",
                "aai.speechmodel.nova" => "nova",
                "aai.speechmodel.universal" => "universal",
                "aai.speechmodel.slam_1" => "slam-1", // Corrected from slap-1 to slam-1
                _ => "nova" // Default to Nova for unknown models
            };
        }
    }
}

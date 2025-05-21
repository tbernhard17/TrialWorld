using System;

namespace TrialWorld.Contracts
{
    /// <summary>
    /// Defines requirements for audio content validation
    /// </summary>
    public class AudioRequirementsDTO
    {
        /// <summary>
        /// Minimum acceptable sample rate in hertz
        /// </summary>
        public required int MinimumSampleRateHz { get; set; }
        
        /// <summary>
        /// Minimum acceptable bitrate in kilobits per second
        /// </summary>
        public required int MinimumBitrateKbps { get; set; }
        
        /// <summary>
        /// Minimum acceptable loudness in decibels
        /// </summary>
        public required float MinimumLoudnessDb { get; set; }
        
        /// <summary>
        /// Maximum acceptable background noise level in decibels
        /// </summary>
        public required float MaximumNoiseLevelDb { get; set; }
        
        /// <summary>
        /// Whether stereo audio is required (as opposed to mono)
        /// </summary>
        public required bool RequireStereo { get; set; }
        
        /// <summary>
        /// Whether vocal presence is required in the audio
        /// </summary>
        public required bool RequireVocalPresence { get; set; }
        
        /// <summary>
        /// Maximum allowed duration for the audio in seconds (0 for no limit)
        /// </summary>
        public int MaxDurationSeconds { get; set; } = 0;
        
        /// <summary>
        /// Allowed audio file formats (e.g., mp3, wav)
        /// </summary>
        public string[]? AllowedFormats { get; set; }
        
        /// <summary>
        /// Creates a default set of audio requirements for standard quality
        /// </summary>
        public static AudioRequirementsDTO CreateDefault()
        {
            return new AudioRequirementsDTO
            {
                MinimumSampleRateHz = 44100,
                MinimumBitrateKbps = 128,
                MinimumLoudnessDb = -30,
                MaximumNoiseLevelDb = -20,
                RequireStereo = false,
                RequireVocalPresence = true,
                AllowedFormats = new[] { "mp3", "wav", "aac", "m4a" }
            };
        }
        
        /// <summary>
        /// Creates a set of audio requirements for high quality
        /// </summary>
        public static AudioRequirementsDTO CreateHighQuality()
        {
            return new AudioRequirementsDTO
            {
                MinimumSampleRateHz = 48000,
                MinimumBitrateKbps = 256,
                MinimumLoudnessDb = -25,
                MaximumNoiseLevelDb = -30,
                RequireStereo = true,
                RequireVocalPresence = true,
                AllowedFormats = new[] { "wav", "aac", "flac" }
            };
        }
    }
}

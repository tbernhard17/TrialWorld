using System;
using System.Collections.Generic;

namespace TrialWorld.Core.Media.Models
{
    /// <summary>
    /// Options for media enhancement operations
    /// </summary>
    public class EnhancementOptions
    {
        #region Basic Properties
        
        /// <summary>
        /// Gets or sets the target resolution (e.g., "1920x1080")
        /// </summary>
        public string TargetResolution { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets whether to enhance video quality
        /// </summary>
        public bool EnhanceVideo { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to enhance audio quality
        /// </summary>
        public bool EnhanceAudio { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to use high-quality encoding (slower but better quality)
        /// </summary>
        public bool UseHighQualityEncoding { get; set; } = false;
        
        /// <summary>
        /// Gets or sets whether to use hardware acceleration if available
        /// </summary>
        public bool UseHardwareAcceleration { get; set; } = true;
        
        #endregion
        
        #region Video Enhancement Properties
        
        /// <summary>
        /// Gets or sets the denoise strength (0-10, where 0 is off and 10 is maximum)
        /// </summary>
        public int DenoiseStrength { get; set; }
        
        /// <summary>
        /// Gets or sets the sharpen strength (0-10, where 0 is off and 10 is maximum)
        /// </summary>
        public int SharpenStrength { get; set; }
        
        /// <summary>
        /// Gets or sets the brightness adjustment (-1.0 to 1.0, where 0 is no change)
        /// </summary>
        public double BrightnessAdjustment { get; set; }
        
        /// <summary>
        /// Gets or sets the contrast adjustment (0.1 to 3.0, where 1.0 is no change)
        /// </summary>
        public double ContrastAdjustment { get; set; } = 1.0;
        
        /// <summary>
        /// Gets or sets the saturation adjustment (0.0 to 3.0, where 1.0 is no change)
        /// </summary>
        public double SaturationAdjustment { get; set; } = 1.0;
        
        /// <summary>
        /// Gets or sets the video quality (0-100)
        /// </summary>
        public int VideoQuality { get; set; } = 85;
        
        #endregion
        
        #region Audio Enhancement Properties
        
        /// <summary>
        /// Gets or sets whether to reduce background noise in audio
        /// </summary>
        public bool ReduceNoise { get; set; } = true;
        
        /// <summary>
        /// Volume adjustment factor (0.1-2.0)
        /// 1.0 = no change, 2.0 = double volume, 0.5 = half volume
        /// </summary>
        public double VolumeBoost { get; set; } = 1.0;
        
        /// <summary>
        /// Gets or sets the audio quality (0-100)
        /// </summary>
        public int AudioQuality { get; set; } = 85;
        
        #endregion
        
        /// <summary>
        /// Creates a new instance of EnhancementOptions with default values
        /// </summary>
        public EnhancementOptions()
        {
            // Set defaults 
            DenoiseStrength = 3;
            SharpenStrength = 2;
            BrightnessAdjustment = 0;
        }
        
        #region Factory Methods
        
        /// <summary>
        /// Helper method to create default enhancement options
        /// </summary>
        public static EnhancementOptions CreateDefault()
        {
            return new EnhancementOptions
            {
                EnhanceVideo = true,
                EnhanceAudio = true,
                ReduceNoise = true,
                VideoQuality = 85,
                AudioQuality = 85,
                UseHardwareAcceleration = true
            };
        }
        
        /// <summary>
        /// Creates a preset for standard video enhancement
        /// </summary>
        /// <returns>EnhancementOptions configured for standard enhancement</returns>
        public static EnhancementOptions CreateStandardPreset()
        {
            return new EnhancementOptions
            {
                DenoiseStrength = 3,
                SharpenStrength = 3,
                BrightnessAdjustment = 0.1,
                ContrastAdjustment = 1.2,
                SaturationAdjustment = 1.1,
                EnhanceAudio = true,
                UseHighQualityEncoding = true
            };
        }
        
        /// <summary>
        /// Creates a preset for high quality video enhancement
        /// </summary>
        /// <returns>EnhancementOptions configured for high quality enhancement</returns>
        public static EnhancementOptions CreateHighQualityPreset()
        {
            return new EnhancementOptions
            {
                DenoiseStrength = 5,
                SharpenStrength = 3,
                BrightnessAdjustment = 0.05,
                ContrastAdjustment = 1.2,
                SaturationAdjustment = 1.15,
                EnhanceAudio = true,
                UseHighQualityEncoding = true
            };
        }
        
        /// <summary>
        /// Creates a preset for cinema-style video enhancement
        /// </summary>
        /// <returns>EnhancementOptions configured for cinema-style enhancement</returns>
        public static EnhancementOptions CreateCinematicPreset()
        {
            return new EnhancementOptions
            {
                DenoiseStrength = 4,
                SharpenStrength = 2,
                BrightnessAdjustment = -0.1,
                ContrastAdjustment = 1.4,
                SaturationAdjustment = 0.9,
                EnhanceAudio = true,
                UseHighQualityEncoding = true
            };
        }
        
        /// <summary>
        /// Creates a preset for vivid video enhancement
        /// </summary>
        /// <returns>EnhancementOptions configured for vivid enhancement</returns>
        public static EnhancementOptions CreateVividPreset()
        {
            return new EnhancementOptions
            {
                DenoiseStrength = 2,
                SharpenStrength = 5,
                BrightnessAdjustment = 0.15,
                ContrastAdjustment = 1.3,
                SaturationAdjustment = 1.4,
                EnhanceAudio = true,
                UseHighQualityEncoding = true
            };
        }
        
        /// <summary>
        /// Creates a preset for low light video enhancement
        /// </summary>
        /// <returns>EnhancementOptions configured for low light enhancement</returns>
        public static EnhancementOptions CreateLowLightPreset()
        {
            return new EnhancementOptions
            {
                DenoiseStrength = 8,
                SharpenStrength = 2,
                BrightnessAdjustment = 0.3,
                ContrastAdjustment = 1.3,
                SaturationAdjustment = 1.1,
                EnhanceAudio = true,
                UseHighQualityEncoding = true
            };
        }
        
        /// <summary>
        /// Creates a preset for audio enhancement
        /// </summary>
        /// <returns>EnhancementOptions configured for audio enhancement</returns>
        public static EnhancementOptions AudioEnhancement()
        {
            return new EnhancementOptions
            {
                EnhanceVideo = false,
                EnhanceAudio = true,
                ReduceNoise = true,
                VolumeBoost = 1.5,
                AudioQuality = 90
            };
        }
        
        #endregion
    }
}
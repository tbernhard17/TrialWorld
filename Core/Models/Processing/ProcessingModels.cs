using System;
using System.Collections.Generic;

namespace TrialWorld.Core.Models.Processing
{
    public class MediaProcessingResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ProcessedFilePath { get; set; } = string.Empty;
    }

    public class ProcessingOptions
    {
        public bool EnableFiltering { get; set; }
        public bool EnableEnhancement { get; set; }

        // Add missing properties based on usage in MediaProcessingService
        public string? VideoCodec { get; set; }
        public string? AudioCodec { get; set; }
        public int? VideoBitrate { get; set; } // in kbps
        public int? AudioBitrate { get; set; } // in kbps
        public int? Width { get; set; } // pixels
        public int? Height { get; set; } // pixels
        public double? FrameRate { get; set; } // fps
        public int? AudioSampleRate { get; set; } // Hz
    }

    public class VideoEnhancementOptions : ProcessingOptions
    {
        public double BrightnessLevel { get; set; }
        public double ContrastLevel { get; set; }
        // Add other enhancement-specific options if needed
    }
}

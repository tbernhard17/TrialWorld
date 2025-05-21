
namespace TrialWorld.Core.Models.Analysis
{
    /// <summary>
    /// Options for customizing media conversion settings.
    /// </summary>
    public class MediaConversionOptions
    {
        public string AudioCodec { get; init; } = "aac";
        public string VideoCodec { get; init; } = "libx264";
        public int AudioBitrate { get; init; } = 192;
        public int VideoBitrate { get; init; } = 2500;
        public int AudioSampleRate { get; init; } = 44100;
        public int VideoFramerate { get; init; } = 30;
        public bool PreserveMetadata { get; init; } = true;

        // Optional video dimensions
        public int? Width { get; init; }
        public int? Height { get; init; }

        // Added for FFmpegService compatibility
        public int FrameRate { get; init; } = 30;
        public string Format { get; init; } = "mp4";
        public int Bitrate { get; init; } = 2500;
    }
}

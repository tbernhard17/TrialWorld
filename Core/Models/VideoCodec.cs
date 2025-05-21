namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Enum for common video codecs used with FFmpeg
    /// </summary>
    public enum VideoCodec
    {
        LibX264,
        LibX265,
        LibVpx,
        LibAom,
        Mpeg4,
        ProRes,
        FFV1,
        Copy
    }
}

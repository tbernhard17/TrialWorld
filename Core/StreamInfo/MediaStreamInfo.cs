using System.Collections.Generic;

namespace TrialWorld.Core.StreamInfo
{
    /// <summary>
    /// Base class for all media stream information.
    /// </summary>
    public abstract class StreamInfo
    {
        /// <summary>
        /// Stream index.
        /// </summary>
        public required int Index { get; set; }

        /// <summary>
        /// Codec name.
        /// </summary>
        public required string Codec { get; set; }

        /// <summary>
        /// Codec long name.
        /// </summary>
        public required string CodecLongName { get; set; }

        /// <summary>
        /// Stream language, if available.
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// Metadata tags as key-value pairs.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new();
    }

    /// <summary>
    /// Information about a video stream.
    /// </summary>
    public class VideoStreamInfo : StreamInfo
    {
        /// <summary>
        /// Width in pixels.
        /// </summary>
        public required int Width { get; set; }

        /// <summary>
        /// Height in pixels.
        /// </summary>
        public required int Height { get; set; }

        /// <summary>
        /// Frame rate in frames per second.
        /// </summary>
        public required double FrameRate { get; set; }

        /// <summary>
        /// Total number of frames, if available.
        /// </summary>
        public long? FrameCount { get; set; }

        /// <summary>
        /// Bitrate in bits per second.
        /// </summary>
        public required long Bitrate { get; set; }

        /// <summary>
        /// Pixel format.
        /// </summary>
        public required string PixelFormat { get; set; }

        /// <summary>
        /// Display aspect ratio as a string (e.g., "16:9").
        /// </summary>
        public required string DisplayAspectRatio { get; set; }
    }

    /// <summary>
    /// Information about an audio stream.
    /// </summary>
    public class AudioStreamInfo : StreamInfo
    {
        /// <summary>
        /// Sample rate in Hz.
        /// </summary>
        public required int SampleRate { get; set; }

        /// <summary>
        /// Number of channels.
        /// </summary>
        public required int Channels { get; set; }

        /// <summary>
        /// Channel layout (e.g., "stereo", "5.1").
        /// </summary>
        public required string ChannelLayout { get; set; }

        /// <summary>
        /// Bitrate in bits per second.
        /// </summary>
        public required long Bitrate { get; set; }

        /// <summary>
        /// Sample format.
        /// </summary>
        public required string SampleFormat { get; set; }
    }

    /// <summary>
    /// Information about a subtitle stream.
    /// </summary>
    public class SubtitleStreamInfo : StreamInfo
    {
        /// <summary>
        /// Subtitle type.
        /// </summary>
        public required string SubtitleType { get; set; }
    }
}
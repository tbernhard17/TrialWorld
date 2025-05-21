using System;
using System.Collections.Generic;

namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Represents a video frame with its associated metadata
    /// </summary>
    public class VideoFrame
    {
        /// <summary>
        /// Raw frame data in the specified pixel format
        /// </summary>
        public required byte[] Data { get; init; }

        /// <summary>
        /// Width of the frame in pixels
        /// </summary>
        public required int Width { get; init; }

        /// <summary>
        /// Height of the frame in pixels
        /// </summary>
        public required int Height { get; init; }

        /// <summary>
        /// Pixel format of the frame data (e.g., "yuv420p", "rgb24")
        /// </summary>
        public required string PixelFormat { get; init; }

        /// <summary>
        /// Frame timestamp in the media's timebase
        /// </summary>
        public required TimeSpan Timestamp { get; init; }

        /// <summary>
        /// Presentation timestamp for display timing
        /// </summary>
        public required TimeSpan PresentationTimestamp { get; init; }

        /// <summary>
        /// Additional metadata associated with the frame
        /// </summary>
        public Dictionary<string, string> Metadata { get; init; } = new();

        /// <summary>
        /// Whether this is a keyframe
        /// </summary>
        public bool IsKeyFrame { get; init; }

        /// <summary>
        /// Picture type (I, P, B frame)
        /// </summary>
        public char PictureType { get; init; }

        /// <summary>
        /// Quality factor of the frame (0-100)
        /// </summary>
        public int Quality { get; init; }

        /// <summary>
        /// Repeat count for frame display
        /// </summary>
        public int RepeatCount { get; init; }

        /// <summary>
        /// Frame flags from FFmpeg
        /// </summary>
        public int Flags { get; init; }
    }

    /// <summary>
    /// Represents audio samples with their associated metadata
    /// </summary>
    public class AudioSamples
    {
        /// <summary>
        /// Raw audio sample data in the specified format
        /// </summary>
        public required byte[] Data { get; init; }

        /// <summary>
        /// Sample rate in Hz
        /// </summary>
        public required int SampleRate { get; init; }

        /// <summary>
        /// Number of audio channels
        /// </summary>
        public required int Channels { get; init; }

        /// <summary>
        /// Audio sample format (e.g., "s16le", "flt")
        /// </summary>
        public required string SampleFormat { get; init; }

        /// <summary>
        /// Timestamp in the media's timebase
        /// </summary>
        public required TimeSpan Timestamp { get; init; }

        /// <summary>
        /// Presentation timestamp for playback timing
        /// </summary>
        public required TimeSpan PresentationTimestamp { get; init; }

        /// <summary>
        /// Number of samples per channel
        /// </summary>
        public required int SamplesPerChannel { get; init; }

        /// <summary>
        /// Channel layout (e.g., "stereo", "5.1")
        /// </summary>
        public string ChannelLayout { get; init; } = "stereo";

        /// <summary>
        /// Additional metadata associated with the samples
        /// </summary>
        public Dictionary<string, string> Metadata { get; init; } = new();

        /// <summary>
        /// Audio level in dB for each channel
        /// </summary>
        public double[] AudioLevels { get; init; } = Array.Empty<double>();

        /// <summary>
        /// Whether these samples contain silence
        /// </summary>
        public bool IsSilent { get; init; }

        /// <summary>
        /// Sample flags from FFmpeg
        /// </summary>
        public int Flags { get; init; }
    }
}
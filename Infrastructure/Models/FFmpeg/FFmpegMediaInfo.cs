using System;
using TrialWorld.Core.StreamInfo; // Required for Video/AudioStreamInfo
using System.Collections.Generic;

namespace TrialWorld.Infrastructure.Models.FFmpeg
{
    /// <summary>
    /// Information about a media file obtained via FFmpeg probing.
    /// Note: Consider merging with MediaInfo if concepts overlap significantly.
    /// </summary>
    public class FFmpegMediaInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public long Bitrate { get; set; }
        public TimeSpan StartTime { get; set; }
        public VideoStreamInfo[] VideoStreams { get; set; } = Array.Empty<VideoStreamInfo>();
        public AudioStreamInfo[] AudioStreams { get; set; } = Array.Empty<AudioStreamInfo>();
    }
}
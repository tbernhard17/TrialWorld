using System;

namespace TrialWorld.Infrastructure.Models.FFmpeg
{
    /// <summary>
    /// Options for configuring FFmpeg operations.
    /// </summary>
    public class FFmpegOptions
    {
        public string? FFmpegPath { get; set; }
        public string? FFprobePath { get; set; }
        public string? FFplayPath { get; set; }
        public string? TempDirectory { get; set; }
        public string? BinaryFolder { get; set; }
        public string? OutputDirectory { get; set; }
        public string? ThumbnailDirectory { get; set; }
        public string? DefaultVideoCodec { get; set; }
        public string? DefaultAudioCodec { get; set; }
        public string? DefaultContainerFormat { get; set; }

        // Properties merged from FFmpegSettings
        public string WorkingDirectory { get; set; } = string.Empty;
        public int MaxConcurrentJobs { get; set; } = 2;
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromMinutes(10);
        public bool EnableLogging { get; set; } = true;

        // Queue monitoring setting
        public int QueuePollingIntervalSeconds { get; set; } = 10;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using TrialWorld.Core.StreamInfo;
using TrialWorld.Core.Models.Transcription;

namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Represents detailed information about a media file
    /// </summary>
    public class MediaInfo
    {
        /// <summary>
        /// Gets or sets the path to the media file
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the creation date of the media file
        /// </summary>
        public DateTime? CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the last modification date of the media file
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the file contains video
        /// </summary>
        public bool HasVideo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the file contains audio
        /// </summary>
        public bool HasAudio { get; set; }

        /// <summary>
        /// Gets or sets additional metadata as key-value pairs
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the title of the media
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Format information.
        /// </summary>
        public MediaFormatInfo? Format { get; set; }

        /// <summary>
        /// List of video streams in the file.
        /// </summary>
        public List<VideoStreamInfo> VideoStreams { get; set; } = new();

        /// <summary>
        /// List of audio streams in the file.
        /// </summary>
        public List<AudioStreamInfo> AudioStreams { get; set; } = new();

        /// <summary>
        /// List of subtitle streams in the file.
        /// </summary>
        public List<TrialWorld.Core.StreamInfo.SubtitleStreamInfo> SubtitleStreams { get; set; } = new();

        /// <summary>
        /// Whether the file contains subtitle content.
        /// </summary>
        public bool HasSubtitles => SubtitleStreams.Count > 0;

        /// <summary>
        /// The primary video stream, or null if there is none.
        /// </summary>
        public VideoStreamInfo? PrimaryVideoStream => VideoStreams.FirstOrDefault();

        /// <summary>
        /// The primary audio stream, or null if there is none.
        /// </summary>
        public AudioStreamInfo? PrimaryAudioStream => AudioStreams.FirstOrDefault();

        /// <summary>
        /// Gets or sets the duration of the media file.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets the duration in seconds (for serialization and compatibility).
        /// </summary>
        public double DurationInSeconds
        {
            get => Duration.TotalSeconds;
            set => Duration = TimeSpan.FromSeconds(value);
        }

        /// <summary>
        /// Creates a new instance of <see cref="MediaInfo"/> with default settings.
        /// </summary>
        public MediaInfo()
        {
            FilePath = string.Empty;
            Title = string.Empty;
            Metadata = new Dictionary<string, string>();
            VideoStreams = new List<VideoStreamInfo>();
            AudioStreams = new List<AudioStreamInfo>();
            SubtitleStreams = new List<TrialWorld.Core.StreamInfo.SubtitleStreamInfo>();
            Format = new MediaFormatInfo
            {
                FormatName = "unknown",
                FormatLongName = "Unknown Format",
                Duration = 0,
                Size = "0",
                BitRate = "0",
                Tags = new Dictionary<string, string>()
            };
            TranscriptSegments = new List<TranscriptSegment>();
        }

        /// <summary>
        /// Gets or sets the ID of the media
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the transcript segments for the media
        /// </summary>
        public List<TranscriptSegment> TranscriptSegments { get; set; } = new();
    }

    /// <summary>
    /// Contains information about the format of a media file.
    /// </summary>
    public class MediaFormatInfo
    {
        /// <summary>
        /// Gets or sets the format name.
        /// </summary>
        public string? FormatName { get; set; }

        /// <summary>
        /// Gets or sets the format long name.
        /// </summary>
        public string? FormatLongName { get; set; }

        /// <summary>
        /// Gets or sets the duration in seconds.
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// Gets or sets the size in bytes.
        /// </summary>
        public string? Size { get; set; }

        /// <summary>
        /// Gets or sets the bitrate in bits per second.
        /// </summary>
        public string? BitRate { get; set; }

        /// <summary>
        /// Gets or sets format-specific tags.
        /// </summary>
        public Dictionary<string, string>? Tags { get; set; }
    }

    /// <summary>
    /// Contains information about an audio stream in a media file.
    /// </summary>
    // Removed duplicate AudioStreamInfo definition. Use Core.StreamInfo.AudioStreamInfo instead.
}
using System;

namespace TrialWorld.Core.Models.Processing
{
    /// <summary>
    /// Contains information about a processed media file
    /// </summary>
    public class ProcessedMediaInfo
    {
        /// <summary>
        /// Gets or sets the path to the original media file
        /// </summary>
        public string OriginalFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the duration of the media file
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets the width of the media file in pixels
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the media file in pixels
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the codec of the video stream
        /// </summary>
        public string VideoCodec { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the codec of the audio stream
        /// </summary>
        public string AudioCodec { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the size of the file in bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets the format of the media file
        /// </summary>
        public string Format { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the media file has a video stream
        /// </summary>
        public bool HasVideo { get; set; }

        /// <summary>
        /// Gets or sets whether the media file has an audio stream
        /// </summary>
        public bool HasAudio { get; set; }
    }
}
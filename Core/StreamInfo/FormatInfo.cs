using System;
using System.Collections.Generic;

namespace TrialWorld.Core.StreamInfo
{
    /// <summary>
    /// Information about a media container format.
    /// </summary>
    public class FormatInfo
    {
        /// <summary>
        /// Gets or sets the format name (e.g., "mp4", "matroska").
        /// </summary>
        public required string FormatName { get; set; }

        /// <summary>
        /// Gets or sets the long name of the format.
        /// </summary>
        public required string FormatLongName { get; set; }

        /// <summary>
        /// Gets or sets the duration of the media in seconds.
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// Gets or sets the file size in bytes.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the overall bitrate of the media in bits per second.
        /// </summary>
        public long Bitrate { get; set; }

        /// <summary>
        /// Gets or sets additional metadata tags for the format.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the bitrate as a string.
        /// </summary>
        public string BitRate => Bitrate.ToString();

        /// <summary>
        /// Gets or sets the size as a string.
        /// </summary>
        public string Size_String => Size.ToString();

        /// <summary>
        /// Gets the duration as a formatted string in "HH:MM:SS.MS" format.
        /// </summary>
        public string FormattedDuration
        {
            get
            {
                int hours = (int)(Duration / 3600);
                int minutes = (int)((Duration % 3600) / 60);
                int seconds = (int)(Duration % 60);
                int milliseconds = (int)((Duration - Math.Floor(Duration)) * 1000);

                return $"{hours:D2}:{minutes:D2}:{seconds:D2}.{milliseconds:D3}";
            }
        }

        /// <summary>
        /// Gets the file size as a human-readable string (e.g., "10.5 MB").
        /// </summary>
        public string FormattedSize
        {
            get
            {
                string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
                int suffixIndex = 0;
                double size = Size;

                while (size >= 1024 && suffixIndex < suffixes.Length - 1)
                {
                    size /= 1024;
                    suffixIndex++;
                }

                return $"{size:0.##} {suffixes[suffixIndex]}";
            }
        }

        /// <summary>
        /// Gets the bitrate as a human-readable string (e.g., "1.5 Mbps").
        /// </summary>
        public string FormattedBitrate
        {
            get
            {
                if (Bitrate < 1000)
                    return $"{Bitrate} bps";

                if (Bitrate < 1000000)
                    return $"{Bitrate / 1000.0:0.#} Kbps";

                return $"{Bitrate / 1000000.0:0.##} Mbps";
            }
        }
    }
}
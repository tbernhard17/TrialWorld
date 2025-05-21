using System;

namespace TrialWorld.Infrastructure.FFmpeg.Codecs
{
    /// <summary>
    /// Enum for common media formats/containers used with FFmpeg
    /// </summary>
    public enum Format
    {
        /// <summary>MP4 container format</summary>
        Mp4,

        /// <summary>WebM container format</summary>
        WebM,

        /// <summary>MKV (Matroska) container format</summary>
        Matroska,

        /// <summary>MP3 audio format</summary>
        Mp3,

        /// <summary>WAV audio format</summary>
        Wav,

        /// <summary>OGG container format</summary>
        Ogg,

        /// <summary>JPEG image format</summary>
        Jpeg,

        /// <summary>PNG image format</summary>
        Png
    }

    /// <summary>
    /// Extension methods for Format enum
    /// </summary>
    public static class FormatExtensions
    {
        /// <summary>
        /// Get the FFmpeg format name for the format enum value
        /// </summary>
        /// <param name="format">Format enum value</param>
        /// <returns>FFmpeg format name string</returns>
        public static string GetFormatName(this Format format)
        {
            return format switch
            {
                Format.Mp4 => "mp4",
                Format.WebM => "webm",
                Format.Matroska => "matroska",
                Format.Mp3 => "mp3",
                Format.Wav => "wav",
                Format.Ogg => "ogg",
                Format.Jpeg => "image2",
                Format.Png => "image2",
                _ => throw new ArgumentException($"Unsupported format: {format}")
            };
        }

        /// <summary>
        /// Get the file extension for the format enum value
        /// </summary>
        /// <param name="format">Format enum value</param>
        /// <returns>File extension string including the dot</returns>
        public static string GetFileExtension(this Format format)
        {
            return format switch
            {
                Format.Mp4 => ".mp4",
                Format.WebM => ".webm",
                Format.Matroska => ".mkv",
                Format.Mp3 => ".mp3",
                Format.Wav => ".wav",
                Format.Ogg => ".ogg",
                Format.Jpeg => ".jpg",
                Format.Png => ".png",
                _ => throw new ArgumentException($"Unsupported format: {format}")
            };
        }
    }
}
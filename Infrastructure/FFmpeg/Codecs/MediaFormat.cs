using System;

namespace TrialWorld.Infrastructure.FFmpeg.Codecs
{
    /// <summary>
    /// Enum for common media container formats used with FFmpeg
    /// </summary>
    public enum MediaFormat
    {
        /// <summary>MP4 container format</summary>
        Mp4,

        /// <summary>MKV (Matroska) container format</summary>
        Mkv,

        /// <summary>WebM container format</summary>
        WebM,

        /// <summary>AVI container format</summary>
        Avi,

        /// <summary>MOV (QuickTime) container format</summary>
        Mov,

        /// <summary>MPEG-TS container format</summary>
        MpegTS,

        /// <summary>MP3 audio format</summary>
        Mp3,

        /// <summary>WAV audio format</summary>
        Wav,

        /// <summary>AAC audio format</summary>
        Aac,

        /// <summary>OGG container format</summary>
        Ogg,

        /// <summary>FLAC audio format</summary>
        Flac,

        /// <summary>JPEG image format</summary>
        Jpeg,

        /// <summary>PNG image format</summary>
        Png,

        /// <summary>BMP image format</summary>
        Bmp
    }

    /// <summary>
    /// Extension methods for MediaFormat enum
    /// </summary>
    public static class MediaFormatExtensions
    {
        /// <summary>
        /// Get the file extension for the specified media format
        /// </summary>
        /// <param name="format">MediaFormat enum value</param>
        /// <returns>File extension string including the dot</returns>
        public static string GetFileExtension(this MediaFormat format)
        {
            return format switch
            {
                MediaFormat.Mp4 => ".mp4",
                MediaFormat.Mkv => ".mkv",
                MediaFormat.WebM => ".webm",
                MediaFormat.Avi => ".avi",
                MediaFormat.Mov => ".mov",
                MediaFormat.MpegTS => ".ts",
                MediaFormat.Mp3 => ".mp3",
                MediaFormat.Wav => ".wav",
                MediaFormat.Aac => ".aac",
                MediaFormat.Ogg => ".ogg",
                MediaFormat.Flac => ".flac",
                MediaFormat.Jpeg => ".jpg",
                MediaFormat.Png => ".png",
                MediaFormat.Bmp => ".bmp",
                _ => throw new ArgumentException($"Unsupported media format: {format}")
            };
        }

        /// <summary>
        /// Get the FFmpeg format name for the specified media format
        /// </summary>
        /// <param name="format">MediaFormat enum value</param>
        /// <returns>FFmpeg format name string</returns>
        public static string GetFormatName(this MediaFormat format)
        {
            return format switch
            {
                MediaFormat.Mp4 => "mp4",
                MediaFormat.Mkv => "matroska",
                MediaFormat.WebM => "webm",
                MediaFormat.Avi => "avi",
                MediaFormat.Mov => "mov",
                MediaFormat.MpegTS => "mpegts",
                MediaFormat.Mp3 => "mp3",
                MediaFormat.Wav => "wav",
                MediaFormat.Aac => "adts",
                MediaFormat.Ogg => "ogg",
                MediaFormat.Flac => "flac",
                MediaFormat.Jpeg => "image2",
                MediaFormat.Png => "image2",
                MediaFormat.Bmp => "image2",
                _ => throw new ArgumentException($"Unsupported media format: {format}")
            };
        }

        /// <summary>
        /// Determine if the specified format is an audio-only format
        /// </summary>
        /// <param name="format">MediaFormat enum value</param>
        /// <returns>True if audio-only format, false otherwise</returns>
        public static bool IsAudioOnly(this MediaFormat format)
        {
            return format is MediaFormat.Mp3 or MediaFormat.Wav
                or MediaFormat.Aac or MediaFormat.Ogg or MediaFormat.Flac;
        }

        /// <summary>
        /// Determine if the specified format is an image format
        /// </summary>
        /// <param name="format">MediaFormat enum value</param>
        /// <returns>True if image format, false otherwise</returns>
        public static bool IsImageFormat(this MediaFormat format)
        {
            return format is MediaFormat.Jpeg or MediaFormat.Png
                or MediaFormat.Bmp;
        }

        /// <summary>
        /// Determine if the specified format is a video container format
        /// </summary>
        /// <param name="format">MediaFormat enum value</param>
        /// <returns>True if video container format, false otherwise</returns>
        public static bool IsVideoContainer(this MediaFormat format)
        {
            return format is MediaFormat.Mp4 or MediaFormat.Mkv
                or MediaFormat.WebM or MediaFormat.Avi
                or MediaFormat.Mov or MediaFormat.MpegTS;
        }
    }
}
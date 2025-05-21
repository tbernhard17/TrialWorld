namespace TrialWorld.Core.Models
{
    public class MediaFormat
    {
        public string? FormatName { get; set; }
        public string? FormatLongName { get; set; }
        public double Duration { get; set; }
        public long Size { get; set; }
        public long Bitrate { get; set; }
        public Dictionary<string, string>? Tags { get; set; }
    }

    public enum MediaFormatEnum
    {
        Mp4,
        Mkv,
        WebM,
        Avi,
        Mov,
        MpegTS,
        Mp3,
        Wav,
        Aac,
        Ogg,
        Flac,
        Jpeg,
        Png,
        Bmp
    }

    public static class MediaFormatExtensions
    {
        public static string GetFileExtension(this MediaFormatEnum format)
        {
            return format switch
            {
                MediaFormatEnum.Mp4 => ".mp4",
                MediaFormatEnum.Mkv => ".mkv",
                MediaFormatEnum.WebM => ".webm",
                MediaFormatEnum.Avi => ".avi",
                MediaFormatEnum.Mov => ".mov",
                MediaFormatEnum.MpegTS => ".ts",
                MediaFormatEnum.Mp3 => ".mp3",
                MediaFormatEnum.Wav => ".wav",
                MediaFormatEnum.Aac => ".aac",
                MediaFormatEnum.Ogg => ".ogg",
                MediaFormatEnum.Flac => ".flac",
                MediaFormatEnum.Jpeg => ".jpg",
                MediaFormatEnum.Png => ".png",
                MediaFormatEnum.Bmp => ".bmp",
                _ => throw new ArgumentException($"Unsupported media format: {format}")
            };
        }

        public static string GetFormatName(this MediaFormatEnum format)
        {
            return format switch
            {
                MediaFormatEnum.Mp4 => "mp4",
                MediaFormatEnum.Mkv => "matroska",
                MediaFormatEnum.WebM => "webm",
                MediaFormatEnum.Avi => "avi",
                MediaFormatEnum.Mov => "mov",
                MediaFormatEnum.MpegTS => "mpegts",
                MediaFormatEnum.Mp3 => "mp3",
                MediaFormatEnum.Wav => "wav",
                MediaFormatEnum.Aac => "adts",
                MediaFormatEnum.Ogg => "ogg",
                MediaFormatEnum.Flac => "flac",
                MediaFormatEnum.Jpeg => "image2",
                MediaFormatEnum.Png => "image2",
                MediaFormatEnum.Bmp => "image2",
                _ => throw new ArgumentException($"Unsupported media format: {format}")
            };
        }

        public static bool IsAudioOnly(this MediaFormatEnum format)
        {
            return format is MediaFormatEnum.Mp3 or MediaFormatEnum.Wav
                or MediaFormatEnum.Aac or MediaFormatEnum.Ogg or MediaFormatEnum.Flac;
        }

        public static bool IsImageFormat(this MediaFormatEnum format)
        {
            return format is MediaFormatEnum.Jpeg or MediaFormatEnum.Png
                or MediaFormatEnum.Bmp;
        }

        public static bool IsVideoContainer(this MediaFormatEnum format)
        {
            return format is MediaFormatEnum.Mp4 or MediaFormatEnum.Mkv
                or MediaFormatEnum.WebM or MediaFormatEnum.Avi
                or MediaFormatEnum.Mov or MediaFormatEnum.MpegTS;
        }
    }
}

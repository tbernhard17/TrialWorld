namespace TrialWorld.Core.Models
{
    public enum Format
    {
        Mp4,
        WebM,
        Matroska,
        Mp3,
        Wav,
        Ogg,
        Jpeg,
        Png
    }

    public static class FormatExtensions
    {
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

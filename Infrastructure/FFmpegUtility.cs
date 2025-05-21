using System;
using System.Collections.Generic;
using System.Text;

namespace TrialWorld.Infrastructure
{
    /// <summary>
    /// Utility for building FFmpeg argument strings in a safe, DRY, and testable way.
    /// </summary>
    public static class FFmpegArgumentBuilder
    {
        public static string BuildInput(string inputPath)
        {
            if (string.IsNullOrWhiteSpace(inputPath))
                throw new ArgumentNullException(nameof(inputPath));
            return $"-i \"{inputPath}\"";
        }

        public static string BuildOutput(string outputPath)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentNullException(nameof(outputPath));
            return $"\"{outputPath}\" -y";
        }

        public static string BuildVideoCodec(string? codec)
        {
            return string.IsNullOrWhiteSpace(codec) ? "-c:v copy" : $"-c:v {codec}";
        }

        public static string BuildAudioCodec(string? codec)
        {
            return string.IsNullOrWhiteSpace(codec) ? "-c:a copy" : $"-c:a {codec}";
        }

        public static string BuildBitrate(string? videoBitrate, string? audioBitrate)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(videoBitrate))
                sb.Append($" -b:v {videoBitrate}");
            if (!string.IsNullOrWhiteSpace(audioBitrate))
                sb.Append($" -b:a {audioBitrate}");
            return sb.ToString();
        }

        public static string BuildFilters(Dictionary<string, string>? filters, string type)
        {
            if (filters == null || filters.Count == 0) return string.Empty;
            var filterList = new List<string>();
            foreach (var filter in filters)
            {
                if (filter.Key.StartsWith(type + ":"))
                    filterList.Add($"{filter.Key.Substring(2)}={filter.Value}");
            }
            if (filterList.Count == 0) return string.Empty;
            var flag = type == "v" ? "-vf" : "-af";
            return $" {flag} \"{string.Join(",", filterList)}\"";
        }
    }
}
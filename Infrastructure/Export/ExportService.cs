using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Processing;
using TrialWorld.Core.Models.Analysis;
using TrialWorld.Core.Exceptions;
using TrialWorld.Core.Services;
using TrialWorld.Infrastructure.Models.FFmpeg;

namespace TrialWorld.Infrastructure.Export
{
    /// <summary>
    /// Implementation of the export service that uses FFmpeg to export clips with enhancements
    /// </summary>
    public class ExportService : IExportService
    {
        private readonly ILogger<ExportService> _logger;
        private readonly IProcessRunner _processRunner;
        private readonly string _ffmpegPath;
        private readonly ExportOptions _options;

        /// <summary>
        /// Gets or sets the quality level for exports (0-100)
        /// </summary>
        public int ExportQuality { get; set; } = 80;

        /// <summary>
        /// Gets or sets whether to include metadata with exports
        /// </summary>
        public bool IncludeMetadata { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the ExportService class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="processRunner">Process runner utility</param>
        /// <param name="options">Export options</param>
        /// <param name="ffmpegOptions">FFmpeg options (to get executable path)</param>
        public ExportService(
            ILogger<ExportService> logger,
            IProcessRunner processRunner,
            IOptions<ExportOptions> options,
            IOptions<FFmpegOptions> ffmpegOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
            _options = options?.Value ?? new ExportOptions();

            if (ffmpegOptions?.Value == null) throw new ArgumentNullException(nameof(ffmpegOptions));
            _ffmpegPath = ffmpegOptions.Value.FFmpegPath ?? "ffmpeg";
            if (!File.Exists(_ffmpegPath))
            {
                _logger.LogCritical("FFmpeg binary not found at path configured for ExportService: {Path}", _ffmpegPath);
                throw new FileNotFoundException($"FFmpeg binary not found at path: {_ffmpegPath}", _ffmpegPath);
            }

            ExportQuality = _options.DefaultQuality;
            IncludeMetadata = _options.IncludeMetadataByDefault;
            _logger.LogInformation("ExportService initialized with Quality={Quality}, IncludeMetadata={Metadata}", ExportQuality, IncludeMetadata);
        }

        /// <summary>
        /// Export a clip with the current enhancement settings
        /// </summary>
        /// <param name="sourcePath">Path to the source media file</param>
        /// <param name="outputPath">Path where the exported file will be saved</param>
        /// <param name="startTime">Start time of the clip to export</param>
        /// <param name="endTime">End time of the clip to export</param>
        /// <param name="videoFilters">Video filters to apply during export</param>
        /// <param name="audioFilters">Audio filters to apply during export</param>
        /// <param name="exportFormat">Format of the exported file</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Path to the exported file if successful, null otherwise</returns>
        public async Task<string?> ExportClipAsync(
            string sourcePath,
            string outputPath,
            TimeSpan startTime,
            TimeSpan endTime,
            VideoFilterChain? videoFilters = null,
            AudioFilterChain? audioFilters = null,
            ExportFormat exportFormat = ExportFormat.MP4,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentException("Source path cannot be empty", nameof(sourcePath));

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path cannot be empty", nameof(outputPath));

            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Source file not found", sourcePath);

            if (endTime <= startTime)
                throw new ArgumentException("End time must be greater than start time");

            try
            {
                _logger.LogInformation("Starting export of clip from {StartTime} to {EndTime} ({Format}) to {OutputPath}", startTime, endTime, exportFormat, outputPath);
                ValidateOutputPath(outputPath);

                var arguments = BuildFFmpegArguments(sourcePath, outputPath, startTime, endTime, videoFilters, audioFilters, exportFormat);
                string ffmpegOutput = string.Empty;
                string ffmpegError = string.Empty;
                int exitCode = -1;

                try
                {
                    ffmpegOutput = await _processRunner.RunProcessAsync(_ffmpegPath, string.Join(" ", arguments), cancellationToken);
                    exitCode = 0;
                }
                catch (ExternalProcessException ex)
                {
                    _logger.LogError(ex, "FFmpeg process failed during export. ExitCode={ExitCode}, Error={Error}", ex.ExitCode, ex.ErrorMessage);
                    exitCode = ex.ExitCode ?? -1;
                    ffmpegError = ex.ErrorMessage ?? string.Empty;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("FFmpeg process cancelled during export for {OutputPath}", outputPath);
                    TryDeleteFile(outputPath);
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error running FFmpeg process for export to {OutputPath}: {Message}", outputPath, ex.Message);
                    TryDeleteFile(outputPath);
                    return null;
                }

                if (exitCode != 0)
                {
                    _logger.LogError("FFmpeg export failed with exit code {ExitCode}. Error Output: {Error}", exitCode, ffmpegError);
                    TryDeleteFile(outputPath);
                    return null;
                }

                _logger.LogInformation("Successfully exported clip to {OutputPath}", outputPath);

                if (IncludeMetadata)
                {
                    var metadata = new ExportMetadata
                    {
                        Title = Path.GetFileNameWithoutExtension(outputPath),
                        SourceFile = sourcePath,
                        StartTime = startTime,
                        EndTime = endTime,
                        VideoFilters = videoFilters,
                        AudioFilters = audioFilters,
                        Format = exportFormat,
                        ExportDate = DateTime.UtcNow
                    };
                    await ExportMetadataAsync(outputPath, metadata, cancellationToken);
                }
                return outputPath;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Export operation cancelled for {OutputPath}", outputPath);
                TryDeleteFile(outputPath);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during export process for {OutputPath}: {Message}", outputPath, ex.Message);
                TryDeleteFile(outputPath);
                return null;
            }
        }

        /// <summary>
        /// Get the default output directory for exports
        /// </summary>
        /// <returns>The default directory path</returns>
        public Task<string> GetDefaultExportDirectoryAsync()
        {
            try
            {
                var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var exportFolder = Path.Combine(documentsFolder, _options.DefaultExportFolderName ?? "TrialWorldExports");
                Directory.CreateDirectory(exportFolder);
                return Task.FromResult(exportFolder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting/creating default export directory. Falling back to MyDocuments. Message: {Message}", ex.Message);
                return Task.FromResult(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            }
        }

        /// <summary>
        /// Export metadata along with the clip
        /// </summary>
        /// <param name="clipPath">Path to the exported clip</param>
        /// <param name="metadata">Metadata to export</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Path to the metadata file if successful, null otherwise</returns>
        public async Task<string?> ExportMetadataAsync(
            string clipPath,
            ExportMetadata metadata,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(clipPath))
                throw new ArgumentException("Clip path cannot be empty", nameof(clipPath));

            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            var metadataPath = Path.ChangeExtension(clipPath, ".metadata.json");

            try
            {
                _logger.LogInformation("Exporting metadata to {MetadataPath}", metadataPath);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                };
                var json = JsonSerializer.Serialize(metadata, options);
                await File.WriteAllTextAsync(metadataPath, json, cancellationToken);
                _logger.LogInformation("Successfully exported metadata to {MetadataPath}", metadataPath);
                return metadataPath;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Metadata export cancelled for {MetadataPath}", metadataPath);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting metadata to {MetadataPath}: {Message}", metadataPath, ex.Message);
                return null;
            }
        }

        #region Helper Methods

        private List<string> BuildFFmpegArguments(
            string sourcePath, string outputPath, TimeSpan startTime, TimeSpan endTime,
            VideoFilterChain? videoFilters, AudioFilterChain? audioFilters, ExportFormat exportFormat)
        {
            var duration = endTime - startTime;
            var arguments = new List<string>
            {
                "-ss", startTime.TotalSeconds.ToString(CultureInfo.InvariantCulture),
                "-i", $"\"{sourcePath}\"",
                "-t", duration.TotalSeconds.ToString(CultureInfo.InvariantCulture),
                "-avoid_negative_ts", "make_zero",
                "-map_metadata", "-1",
                "-map_chapters", "-1"
            };

            var currentVideoFilters = videoFilters?.GetFilters();
            if (currentVideoFilters != null && currentVideoFilters.Any())
            {
                arguments.Add("-vf");
                arguments.Add(string.Join(",", currentVideoFilters.Select(f => f.ToString())));
            }

            var currentAudioFilters = audioFilters?.GetFilters();
            if (currentAudioFilters != null && currentAudioFilters.Any())
            {
                arguments.Add("-af");
                arguments.Add(string.Join(",", currentAudioFilters.Select(f => f.ToString())));
            }

            AddEncodingSettings(arguments, exportFormat);

            if (exportFormat == ExportFormat.MOV)
            {
                arguments.Add("-movflags");
                arguments.Add("+faststart");
            }

            arguments.Add($"\"{outputPath}\"");

            return arguments;
        }

        private ExportFormat GetFormatFromExtension(string filePath)
        {
            var ext = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant();
            return ext switch
            {
                "mp4" => ExportFormat.MP4,
                "mkv" => ExportFormat.MKV,
                "webm" => ExportFormat.WebM,
                "mov" => ExportFormat.MOV,
                "mp3" => ExportFormat.MP3,
                "aac" => ExportFormat.AAC,
                "wav" => ExportFormat.WAV,
                _ => ExportFormat.MP4
            };
        }

        private void AddEncodingSettings(List<string> arguments, ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.MP4:
                case ExportFormat.MOV:
                case ExportFormat.MKV:
                    arguments.AddRange(new[] { "-c:v", "libx264", "-preset", "medium", "-crf", CalculateCrf(ExportQuality, 18, 28).ToString() });
                    arguments.AddRange(new[] { "-c:a", "aac", "-b:a", CalculateAudioBitrate(ExportQuality, 96, 256).ToString() + "k" });
                    break;
                case ExportFormat.WebM:
                    arguments.AddRange(new[] { "-c:v", "libvpx-vp9", "-crf", CalculateCrf(ExportQuality, 15, 35).ToString(), "-b:v", "0" });
                    arguments.AddRange(new[] { "-c:a", "libopus", "-b:a", CalculateAudioBitrate(ExportQuality, 64, 192).ToString() + "k" });
                    break;
                case ExportFormat.MP3:
                    arguments.AddRange(new[] { "-vn", "-c:a", "libmp3lame", "-q:a", CalculateLameQuality(ExportQuality).ToString() });
                    break;
                case ExportFormat.AAC:
                    arguments.AddRange(new[] { "-vn", "-c:a", "aac", "-b:a", CalculateAudioBitrate(ExportQuality, 96, 256).ToString() + "k" });
                    break;
                case ExportFormat.WAV:
                    arguments.AddRange(new[] { "-vn", "-c:a", "pcm_s16le" });
                    break;
            }
        }

        private int CalculateCrf(int quality, int bestCrf, int worstCrf)
        {
            quality = Math.Clamp(quality, 0, 100);
            return worstCrf - (int)Math.Round((worstCrf - bestCrf) * (quality / 100.0));
        }

        private int CalculateAudioBitrate(int quality, int minBitrate, int maxBitrate)
        {
            quality = Math.Clamp(quality, 0, 100);
            return minBitrate + (int)Math.Round((maxBitrate - minBitrate) * (quality / 100.0));
        }

        private int CalculateLameQuality(int quality)
        {
            quality = Math.Clamp(quality, 0, 100);
            return 9 - (int)Math.Round(9 * (quality / 100.0));
        }

        private void TryDeleteFile(string? filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                try { File.Delete(filePath); }
                catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete incomplete output file: {FilePath}", filePath); }
            }
        }

        private void ValidateOutputPath(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to create output directory '{dir}': {ex.Message}", ex);
                }
            }
        }

        #endregion
    }

    public class FFmpegOptions
    {
        public string? FFmpegPath { get; set; }
        public string? FFplayPath { get; set; }
        public string? TempDirectory { get; set; }
        public string? BinaryFolder { get; set; }
        public string? OutputDirectory { get; set; }
        public string? ThumbnailDirectory { get; set; }
        public string? DefaultVideoCodec { get; set; }
        public string? DefaultAudioCodec { get; set; }
        public string? DefaultContainerFormat { get; set; }
    }
}
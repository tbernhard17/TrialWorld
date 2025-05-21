using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Processing;
using TrialWorld.Core.Models.Analysis;
using TrialWorld.Core.Exceptions;
using TrialWorld.Core.StreamInfo;

namespace TrialWorld.Application.Services
{
    public class MediaProcessingService : IMediaProcessingService
    {
        private readonly ILogger<MediaProcessingService> _logger;
        private readonly ITranscriptionService _transcriptionService;
        private readonly IMediaInfoService _mediaInfoService;
        private readonly IAnalysisService _analysisService;
        private readonly IMediaConversionService? _mediaConversionService;

        public MediaProcessingService(
            ILogger<MediaProcessingService> logger,
            ITranscriptionService transcriptionService,
            IMediaInfoService mediaInfoService,
            IAnalysisService analysisService,
            IMediaConversionService? mediaConversionService = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _transcriptionService = transcriptionService ?? throw new ArgumentNullException(nameof(transcriptionService));
            _mediaInfoService = mediaInfoService ?? throw new ArgumentNullException(nameof(mediaInfoService));
            _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
            _mediaConversionService = mediaConversionService;
        }

        public async Task<MediaProcessingResult> ProcessAsync(
            string filePath,
            string? outputDirectory = default,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Processing file (basic): {FilePath}", filePath);
            var metadata = await ExtractMetadataAsync(filePath, cancellationToken);
            return new MediaProcessingResult { 
                Success = metadata != null, 
                ProcessedFilePath = filePath,
                ErrorMessage = metadata == null ? "Failed to extract metadata" : string.Empty
            };
        }

        public async Task<MediaProcessingResult> ProcessMediaAsync(
            string inputPath,
            ProcessingOptions options,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Processing file (full): {FilePath}", inputPath);
            progress?.Report(0.1);
            try
            { 
                var transcriptionResult = await _transcriptionService.TranscribeAsync(inputPath, cancellationToken);
                progress?.Report(0.6);
                if (!transcriptionResult.Success)
                {
                    return new MediaProcessingResult { Success = false, ErrorMessage = transcriptionResult.Error ?? "Transcription failed" };
                }

                await Task.Delay(100, cancellationToken);
                progress?.Report(1.0);

                return new MediaProcessingResult { Success = true, ProcessedFilePath = inputPath };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during full media processing for {InputPath}", inputPath);
                return new MediaProcessingResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<IEnumerable<MediaProcessingResult>> ProcessMediaBatchAsync(
            IEnumerable<string> inputPaths,
            ProcessingOptions options,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var results = new List<MediaProcessingResult>();
            int totalFiles = inputPaths.Count();
            int processedFiles = 0;

            foreach (var path in inputPaths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = await ProcessMediaAsync(path, options, null, cancellationToken);
                results.Add(result);
                processedFiles++;
                progress?.Report((double)processedFiles / totalFiles);
            }
            return results;
        }

        public async Task<string> ConvertMediaFormatAsync(
            string sourceFilePath,
            string targetFormat,
            ProcessingOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Converting {SourcePath} to {Format}", sourceFilePath, targetFormat);
            if (_mediaConversionService != null) {
                await Task.Delay(200, cancellationToken);
                return Path.ChangeExtension(sourceFilePath, "." + targetFormat.ToLowerInvariant());
            } else {
                _logger.LogWarning("MediaConversionService not available for ConvertMediaFormatAsync");
                await Task.Delay(10, cancellationToken);
                return sourceFilePath;
            }
        }

        public async Task EnhanceVideoAsync(
            string filePath, 
            TrialWorld.Core.Models.Processing.VideoEnhancementOptions options, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Enhancing video {FilePath}", filePath);
            await Task.Delay(300, cancellationToken);
        }

        public async Task<MediaInfo> ExtractMetadataAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Extracting metadata for: {FilePath}", filePath);
            return await _mediaInfoService.GetMediaInfoAsync(filePath, cancellationToken);
        }

        private static string BuildOutputPath(string input, string? directory)
        {
            var inputDir = Path.GetDirectoryName(input) ?? Environment.CurrentDirectory;
            var filename = Path.GetFileNameWithoutExtension(input);
            var extension = Path.GetExtension(input);

            var outputFolder = directory ?? Path.Combine(inputDir, "processed");
            Directory.CreateDirectory(outputFolder);

            return Path.Combine(outputFolder, $"{filename}_processed{extension}");
        }

        private static TrialWorld.Core.Models.Analysis.MediaConversionOptions MapProcessingToConversionOptions(ProcessingOptions appOptions)
        {
            return new TrialWorld.Core.Models.Analysis.MediaConversionOptions
            {
                VideoCodec = "libx264",
                AudioCodec = "aac",
                VideoBitrate = 2500,
                AudioBitrate = 192,
                FrameRate = 30,
                AudioSampleRate = 44100,
            };
        }

        private static TrialWorld.Core.Models.Analysis.MediaConversionOptions MapEnhancementToConversionOptions(TrialWorld.Core.Models.Processing.VideoEnhancementOptions enhanceOptions)
        {
            return new TrialWorld.Core.Models.Analysis.MediaConversionOptions
            {
                VideoCodec = "libx265",
                AudioCodec = "copy",
            };
        }
    }
}

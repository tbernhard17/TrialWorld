using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrialWorld.Core.Interfaces;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrialWorld.Core.Models.Analysis;
using TrialWorld.Core.Models;
using TrialWorld.Infrastructure.Services;
using TrialWorld.Infrastructure.Models.FFmpeg;

namespace TrialWorld.Infrastructure.FFmpeg.Tests
{
    [TestClass]
    public class FFmpegServiceTests
    {
        private Mock<ILogger<FFmpegService>> _mockLogger = null!;
        private Mock<IOptions<FFmpegOptions>> _mockOptions = null!;
        private Mock<IProcessRunner> _mockProcessRunner = null!;
        private Mock<IMediaInfoService> _mockMediaInfoService = null!;

        private IFFmpegService _ffmpegService = null!;
        private string _testDirectory = string.Empty;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<FFmpegService>>();
            _mockOptions = new Mock<IOptions<FFmpegOptions>>();
            _mockProcessRunner = new Mock<IProcessRunner>();
            _mockMediaInfoService = new Mock<IMediaInfoService>();

            _mockOptions.Setup(o => o.Value).Returns(new FFmpegOptions { FFmpegPath = "ffmpeg", FFprobePath = "ffprobe" });

            _ffmpegService = new FFmpegService(
                _mockLogger.Object,
                _mockOptions.Object,
                _mockProcessRunner.Object,
                _mockMediaInfoService.Object);

            _testDirectory = Path.Combine(Path.GetTempPath(), "FFmpegTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDirectory);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testDirectory))
            {
                try
                {
                    Directory.Delete(_testDirectory, true);
                }
                catch (IOException)
                {
                    // Ignore errors during cleanup
                }
            }
        }

        // TODO: Add NEW unit/integration tests focusing ONLY on ExecuteCommandAsync
        // Example:
        /*
        [TestMethod]
        public async Task ExecuteCommandAsync_ValidCommand_ReturnsSuccessResult()
        {
            // Arrange
            var expectedResult = new FFmpegResult { ExitCode = 0, Output = "Success", Error = "" };
            _mockProcessRunner.Setup(p => p.RunProcessAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(expectedResult);
            string arguments = "-i input.mp4 output.mp4";

            // Act
            var result = await _ffmpegService.ExecuteCommandAsync(arguments, CancellationToken.None);

            // Assert
            Assert.AreEqual(0, result.ExitCode);
            Assert.AreEqual("Success", result.Output);
            _mockProcessRunner.Verify(p => p.RunProcessAsync("ffmpeg", arguments, It.IsAny<CancellationToken>()), Times.Once);
        }
        */

        // --- REMOVED ALL PREVIOUS INTEGRATION TESTS --- 
        // These tests are no longer valid for FFmpegService after refactoring.
        // The tested functionality now belongs to specific services like
        // IMediaInfoService, IThumbnailExtractor, IAudioExtractorService, IMediaTrimmer, etc.
        // Ensure those services have appropriate tests.

        // Removed: ProbeMediaAsync_ValidVideoFile_ReturnsCorrectMediaInfo
        // Removed: ExtractThumbnailsAsync_ValidVideoFile_ExtractsCorrectNumberOfThumbnails
        // Removed: ExtractAudioAsync_ValidVideoFile_ExtractsAudio
        // Removed: TrimMediaAsync_ValidVideoFile_TrimsCorrectly
        // Removed: ConvertMediaAsync_ValidVideoFile_ConvertsCorrectly
        // Removed: CreateGifFromVideoAsync_ValidVideoFile_CreatesGif (if it existed)
        // Removed: GenerateWaveformDataAsync_ValidAudioFile_GeneratesWaveformData
        // Removed: ConcatenateMediaAsync_MultipleValidFiles_ConcatenatesCorrectly
        // Removed: GetFFmpegVersionAsync_ReturnsNonEmptyString (Unless ExecuteCommandAsync is used)
    }
}
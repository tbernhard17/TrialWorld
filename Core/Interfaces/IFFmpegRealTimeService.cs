using System;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Interface for real-time FFmpeg operations with hardware acceleration support.
    /// </summary>
    public interface IFFmpegRealTimeService
    {
        /// <summary>
        /// Gets whether the service is currently processing a stream
        /// </summary>
        bool IsProcessing { get; }

        /// <summary>
        /// Gets the number of frames processed
        /// </summary>
        long ProcessedFrames { get; }

        /// <summary>
        /// Gets the number of frames dropped
        /// </summary>
        long DroppedFrames { get; }

        /// <summary>
        /// Gets or sets the playback rate (1.0 = normal speed)
        /// </summary>
        double PlaybackRate { get; set; }

        /// <summary>
        /// Initializes the service with the specified input file
        /// </summary>
        /// <param name="inputPath">Path to the input media file</param>
        /// <param name="accelerationMode">Hardware acceleration mode to use</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        Task<bool> InitializeAsync(string inputPath, HardwareAccelerationMode accelerationMode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts processing a stream with the specified parameters
        /// </summary>
        /// <param name="inputUrl">URL of the input stream</param>
        /// <param name="outputUrl">URL where the processed stream will be output</param>
        /// <param name="videoFilters">Optional video filters to apply</param>
        /// <param name="audioFilters">Optional audio filters to apply</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        Task StartStreamProcessingAsync(string inputUrl, string outputUrl, VideoFilterChain? videoFilters = null, AudioFilterChain? audioFilters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops the current stream processing
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        Task StopStreamProcessingAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the video filters for the current stream
        /// </summary>
        /// <param name="filters">New video filters to apply</param>
        void UpdateVideoFilters(VideoFilterChain filters);

        /// <summary>
        /// Updates the audio filters for the current stream
        /// </summary>
        /// <param name="filters">New audio filters to apply</param>
        void UpdateAudioFilters(AudioFilterChain filters);

        /// <summary>
        /// Gets the next video frame with optional real-time filtering
        /// </summary>
        /// <param name="filters">Optional video filters to apply</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        Task<VideoFrame> GetNextFrameAsync(VideoFilterChain? filters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the next audio samples with optional real-time filtering
        /// </summary>
        /// <param name="filters">Optional audio filters to apply</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        Task<AudioSamples> GetNextAudioSamplesAsync(AudioFilterChain? filters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current latency of the stream processing
        /// </summary>
        /// <returns>Latency in milliseconds</returns>
        Task<double> GetCurrentLatencyAsync();

        /// <summary>
        /// Gets information about available hardware acceleration
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        Task<HardwareAccelerationInfo> GetHardwareAccelerationInfoAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Seeks to the specified position in the media
        /// </summary>
        /// <param name="position">Position to seek to</param>
        /// <param name="mode">Seek mode to use</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        Task<bool> SeekAsync(TimeSpan position, SeekMode mode = SeekMode.Accurate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Releases all resources used by the service
        /// </summary>
        Task DisposeAsync();
    }

    public class HardwareAccelerationInfo
    {
        public required HardwareAccelerationMode[] SupportedModes { get; init; }
        public required string[] AvailableDecoders { get; init; }
        public required string[] AvailableEncoders { get; init; }
        public required Dictionary<string, string> Capabilities { get; init; }
    }
}
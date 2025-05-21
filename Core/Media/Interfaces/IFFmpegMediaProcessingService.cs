using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models.Processing;

namespace TrialWorld.Core.Media.Interfaces
{
    /// <summary>
    /// Interface for advanced media processing operations using FFmpeg.
    /// Handles media enhancement, transformation, and filter application for
    /// court recordings, streaming, and production environments.
    /// </summary>
    public interface IFFmpegMediaProcessingService
    {
        #region Media Enhancement

        /// <summary>
        /// Enhances audio quality by applying multiple filters optimized for voice clarity
        /// </summary>
        /// <param name="inputPath">Path to input media file</param>
        /// <param name="outputPath">Path for enhanced output</param>
        /// <param name="options">Enhancement options including noise reduction level, normalization settings</param>
        /// <param name="progressCallback">Optional callback for progress reporting</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the enhancement operation</returns>
        Task<MediaProcessingResult> EnhanceAudioAsync(
            string inputPath,
            string outputPath,
            AudioEnhancementOptions options,
            IProgress<double>? progressCallback = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Enhances video quality with filters optimized for clarity and stabilization
        /// </summary>
        /// <param name="inputPath">Path to input media file</param>
        /// <param name="outputPath">Path for enhanced output</param>
        /// <param name="options">Enhancement options including denoiser strength, sharpening level</param>
        /// <param name="progressCallback">Optional callback for progress reporting</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the enhancement operation</returns>
        Task<MediaProcessingResult> EnhanceVideoAsync(
            string inputPath,
            string outputPath,
            VideoEnhancementOptions options,
            IProgress<double>? progressCallback = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Normalizes audio levels to ensure consistent volume
        /// </summary>
        /// <param name="inputPath">Path to input media file</param>
        /// <param name="outputPath">Path for normalized output</param>
        /// <param name="targetLevel">Target loudness level in LUFS</param>
        /// <param name="progressCallback">Optional callback for progress reporting</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the normalization operation</returns>
        Task<MediaProcessingResult> NormalizeAudioAsync(
            string inputPath,
            string outputPath,
            double targetLevel = -16.0,
            IProgress<double>? progressCallback = null,
            CancellationToken cancellationToken = default);

        #endregion

        #region Real-time Processing

        /// <summary>
        /// Creates a real-time processing session for media enhancement
        /// </summary>
        /// <param name="inputPath">Path to input media file</param>
        /// <param name="initialOptions">Initial processing options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Session ID for the real-time processing session</returns>
        Task<string> CreateRealTimeProcessingSessionAsync(
            string inputPath,
            MediaProcessingOptions initialOptions,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates audio filters for an active real-time processing session
        /// </summary>
        /// <param name="sessionId">Session ID from CreateRealTimeProcessingSessionAsync</param>
        /// <param name="audioFilters">New audio filters to apply</param>
        /// <returns>True if filter update was successful</returns>
        Task<bool> UpdateRealTimeAudioFiltersAsync(
            string sessionId,
            IEnumerable<FilterSettings> audioFilters);

        /// <summary>
        /// Updates video filters for an active real-time processing session
        /// </summary>
        /// <param name="sessionId">Session ID from CreateRealTimeProcessingSessionAsync</param>
        /// <param name="videoFilters">New video filters to apply</param>
        /// <returns>True if filter update was successful</returns>
        Task<bool> UpdateRealTimeVideoFiltersAsync(
            string sessionId,
            IEnumerable<FilterSettings> videoFilters);

        /// <summary>
        /// Gets the preview URL for a real-time processing session
        /// </summary>
        /// <param name="sessionId">Session ID from CreateRealTimeProcessingSessionAsync</param>
        /// <returns>URL for accessing the preview stream</returns>
        Task<string> GetRealTimePreviewUrlAsync(string sessionId);

        /// <summary>
        /// Closes a real-time processing session and releases resources
        /// </summary>
        /// <param name="sessionId">Session ID from CreateRealTimeProcessingSessionAsync</param>
        /// <returns>True if session was successfully closed</returns>
        Task<bool> CloseRealTimeProcessingSessionAsync(string sessionId);

        #endregion

        #region Advanced Audio Processing

        /// <summary>
        /// Reduces background noise in audio using adaptive noise reduction techniques
        /// </summary>
        /// <param name="inputPath">Path to input media file</param>
        /// <param name="outputPath">Path for noise-reduced output</param>
        /// <param name="strength">Noise reduction strength (0.0-1.0)</param>
        /// <param name="progressCallback">Optional callback for progress reporting</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the noise reduction operation</returns>
        Task<MediaProcessingResult> ReduceNoiseAsync(
            string inputPath,
            string outputPath,
            double strength = 0.5,
            IProgress<double>? progressCallback = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes echo/reverb from audio recording
        /// </summary>
        /// <param name="inputPath">Path to input media file</param>
        /// <param name="outputPath">Path for processed output</param>
        /// <param name="strength">Echo cancellation strength (0.0-1.0)</param>
        /// <param name="progressCallback">Optional callback for progress reporting</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the echo cancellation operation</returns>
        Task<MediaProcessingResult> CancelEchoAsync(
            string inputPath,
            string outputPath,
            double strength = 0.5,
            IProgress<double>? progressCallback = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Enhances speech clarity in audio recordings
        /// </summary>
        /// <param name="inputPath">Path to input media file</param>
        /// <param name="outputPath">Path for enhanced output</param>
        /// <param name="options">Speech enhancement options</param>
        /// <param name="progressCallback">Optional callback for progress reporting</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the speech enhancement operation</returns>
        Task<MediaProcessingResult> EnhanceSpeechAsync(
            string inputPath,
            string outputPath,
            SpeechEnhancementOptions options,
            IProgress<double>? progressCallback = null,
            CancellationToken cancellationToken = default);

        #endregion

        #region Media Transformation

        /// <summary>
        /// Trims a media file to extract a specific segment
        /// </summary>
        /// <param name="inputPath">Path to input media file</param>
        /// <param name="outputPath">Path for trimmed output</param>
        /// <param name="startTime">Start time of the segment</param>
        /// <param name="duration">Duration of the segment</param>
        /// <param name="progressCallback">Optional callback for progress reporting</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the trimming operation</returns>
        Task<MediaProcessingResult> TrimMediaAsync(
            string inputPath,
            string outputPath,
            TimeSpan startTime,
            TimeSpan duration,
            IProgress<double>? progressCallback = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Joins multiple media files into a single output file
        /// </summary>
        /// <param name="inputPaths">List of input file paths in order</param>
        /// <param name="outputPath">Path for joined output</param>
        /// <param name="options">Joining options</param>
        /// <param name="progressCallback">Optional callback for progress reporting</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the join operation</returns>
        Task<MediaProcessingResult> JoinMediaFilesAsync(
            IList<string> inputPaths,
            string outputPath,
            MediaJoinOptions options,
            IProgress<double>? progressCallback = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Converts media to a different format with specified options
        /// </summary>
        /// <param name="inputPath">Path to input media file</param>
        /// <param name="outputPath">Path for converted output</param>
        /// <param name="options">Conversion options</param>
        /// <param name="progressCallback">Optional callback for progress reporting</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the conversion operation</returns>
        Task<MediaProcessingResult> ConvertMediaFormatAsync(
            string inputPath,
            string outputPath,
            MediaConversionOptions options,
            IProgress<double>? progressCallback = null,
            CancellationToken cancellationToken = default);

        #endregion

        #region Analysis and Extraction

        /// <summary>
        /// Extracts audio from a video file
        /// </summary>
        /// <param name="inputPath">Path to input video file</param>
        /// <param name="outputPath">Path for extracted audio</param>
        /// <param name="audioCodec">Audio codec to use</param>
        /// <param name="progressCallback">Optional callback for progress reporting</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the extraction operation</returns>
        Task<MediaProcessingResult> ExtractAudioAsync(
            string inputPath,
            string outputPath,
            string audioCodec = "aac",
            IProgress<double>? progressCallback = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Extracts a still frame from a video at a specific time point
        /// </summary>
        /// <param name="inputPath">Path to input video file</param>
        /// <param name="outputPath">Path for extracted frame</param>
        /// <param name="timePosition">Time position to extract frame from</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the frame extraction operation</returns>
        Task<MediaProcessingResult> ExtractFrameAsync(
            string inputPath,
            string outputPath,
            TimeSpan timePosition,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a waveform visualization for an audio file
        /// </summary>
        /// <param name="inputPath">Path to input audio file</param>
        /// <param name="outputPath">Path for waveform image</param>
        /// <param name="options">Waveform visualization options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the waveform generation operation</returns>
        Task<MediaProcessingResult> GenerateWaveformAsync(
            string inputPath,
            string outputPath,
            WaveformOptions options,
            CancellationToken cancellationToken = default);

        #endregion

        #region Filter Management

        /// <summary>
        /// Gets a list of available audio filters
        /// </summary>
        /// <returns>Collection of available audio filters with descriptions</returns>
        Task<IEnumerable<FilterInfo>> GetAvailableAudioFiltersAsync();

        /// <summary>
        /// Gets a list of available video filters
        /// </summary>
        /// <returns>Collection of available video filters with descriptions</returns>
        Task<IEnumerable<FilterInfo>> GetAvailableVideoFiltersAsync();

        /// <summary>
        /// Checks if a specific filter is available and supported
        /// </summary>
        /// <param name="filterName">Name of the filter to check</param>
        /// <param name="filterType">Type of filter (audio or video)</param>
        /// <returns>True if filter is available</returns>
        Task<bool> IsFilterAvailableAsync(string filterName, FilterType filterType);

        /// <summary>
        /// Gets detailed information about a specific filter
        /// </summary>
        /// <param name="filterName">Name of the filter</param>
        /// <param name="filterType">Type of filter (audio or video)</param>
        /// <returns>Detailed filter information or null if filter not found</returns>
        Task<FilterInfo?> GetFilterInfoAsync(string filterName, FilterType filterType);

        #endregion
    }

    /// <summary>
    /// Type of media filter
    /// </summary>
    public enum FilterType
    {
        /// <summary>
        /// Audio filter
        /// </summary>
        Audio,

        /// <summary>
        /// Video filter
        /// </summary>
        Video
    }

    /// <summary>
    /// Information about an FFmpeg filter
    /// </summary>
    public class FilterInfo
    {
        /// <summary>
        /// Name of the filter
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Type of the filter (audio or video)
        /// </summary>
        public FilterType Type { get; set; }

        /// <summary>
        /// Description of the filter
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Parameters that can be configured for this filter
        /// </summary>
        public IList<FilterParameterInfo> Parameters { get; set; } = new List<FilterParameterInfo>();
    }

    /// <summary>
    /// Information about a filter parameter
    /// </summary>
    public class FilterParameterInfo
    {
        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the parameter
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Data type of the parameter
        /// </summary>
        public string DataType { get; set; } = string.Empty;

        /// <summary>
        /// Default value of the parameter
        /// </summary>
        public string DefaultValue { get; set; } = string.Empty;

        /// <summary>
        /// Minimum value (for numeric parameters)
        /// </summary>
        public string? MinValue { get; set; }

        /// <summary>
        /// Maximum value (for numeric parameters)
        /// </summary>
        public string? MaxValue { get; set; }

        /// <summary>
        /// Allowed values (for enum-like parameters)
        /// </summary>
        public IList<string>? AllowedValues { get; set; }
    }

    /// <summary>
    /// Settings for a specific filter
    /// </summary>
    public class FilterSettings
    {
        /// <summary>
        /// Name of the filter
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Filter-specific parameters
        /// </summary>
        public IDictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Options for audio enhancement
    /// </summary>
    public class AudioEnhancementOptions
    {
        /// <summary>
        /// Level of noise reduction (0.0-1.0)
        /// </summary>
        public double NoiseReductionLevel { get; set; } = 0.3;

        /// <summary>
        /// Whether to normalize audio levels
        /// </summary>
        public bool Normalize { get; set; } = true;

        /// <summary>
        /// Target loudness level in LUFS
        /// </summary>
        public double TargetLoudness { get; set; } = -16.0;

        /// <summary>
        /// Whether to enhance speech clarity
        /// </summary>
        public bool EnhanceSpeech { get; set; } = true;

        /// <summary>
        /// Level of dynamic range compression (0.0-1.0)
        /// </summary>
        public double CompressionLevel { get; set; } = 0.5;

        /// <summary>
        /// Whether to apply echo cancellation
        /// </summary>
        public bool CancelEcho { get; set; } = false;

        /// <summary>
        /// Additional custom audio filters
        /// </summary>
        public IList<FilterSettings> CustomFilters { get; set; } = new List<FilterSettings>();
    }

    /// <summary>
    /// Options for video enhancement
    /// </summary>
    public class VideoEnhancementOptions
    {
        /// <summary>
        /// Level of denoising (0.0-1.0)
        /// </summary>
        public double DenoiseLevel { get; set; } = 0.3;

        /// <summary>
        /// Level of sharpening (0.0-1.0)
        /// </summary>
        public double SharpenLevel { get; set; } = 0.2;

        /// <summary>
        /// Whether to apply video stabilization
        /// </summary>
        public bool Stabilize { get; set; } = false;

        /// <summary>
        /// Whether to enhance contrast
        /// </summary>
        public bool EnhanceContrast { get; set; } = true;

        /// <summary>
        /// Whether to apply color correction
        /// </summary>
        public bool ColorCorrection { get; set; } = true;

        /// <summary>
        /// Whether to upscale low-resolution video
        /// </summary>
        public bool Upscale { get; set; } = false;

        /// <summary>
        /// Target resolution width for upscaling (null for automatic)
        /// </summary>
        public int? TargetWidth { get; set; }

        /// <summary>
        /// Target resolution height for upscaling (null for automatic)
        /// </summary>
        public int? TargetHeight { get; set; }

        /// <summary>
        /// Additional custom video filters
        /// </summary>
        public IList<FilterSettings> CustomFilters { get; set; } = new List<FilterSettings>();
    }

    /// <summary>
    /// Options for speech enhancement
    /// </summary>
    public class SpeechEnhancementOptions
    {
        /// <summary>
        /// Whether to apply bandpass filtering optimized for human voice
        /// </summary>
        public bool BandpassFilter { get; set; } = true;

        /// <summary>
        /// Whether to apply de-essing (reduce sibilance)
        /// </summary>
        public bool DeEsser { get; set; } = true;

        /// <summary>
        /// Whether to apply noise gate to reduce background noise
        /// </summary>
        public bool NoiseGate { get; set; } = true;

        /// <summary>
        /// Whether to apply compression to even out volume levels
        /// </summary>
        public bool Compression { get; set; } = true;

        /// <summary>
        /// Threshold for noise gate (in dB)
        /// </summary>
        public double NoiseGateThreshold { get; set; } = -30.0;

        /// <summary>
        /// High-pass filter frequency (Hz) to reduce low-frequency noise
        /// </summary>
        public int HighPassFrequency { get; set; } = 80;

        /// <summary>
        /// Low-pass filter frequency (Hz) to reduce high-frequency noise
        /// </summary>
        public int LowPassFrequency { get; set; } = 12000;
    }

    /// <summary>
    /// Options for media processing
    /// </summary>
    public class MediaProcessingOptions
    {
        /// <summary>
        /// Audio enhancement options
        /// </summary>
        public AudioEnhancementOptions? AudioOptions { get; set; }

        /// <summary>
        /// Video enhancement options
        /// </summary>
        public VideoEnhancementOptions? VideoOptions { get; set; }

        /// <summary>
        /// Output format (e.g., "mp4", "mov", "mkv")
        /// </summary>
        public string OutputFormat { get; set; } = "mp4";

        /// <summary>
        /// Video codec to use
        /// </summary>
        public string VideoCodec { get; set; } = "libx264";

        /// <summary>
        /// Audio codec to use
        /// </summary>
        public string AudioCodec { get; set; } = "aac";

        /// <summary>
        /// Video bitrate (e.g., "2M", "5000k")
        /// </summary>
        public string? VideoBitrate { get; set; }

        /// <summary>
        /// Audio bitrate (e.g., "128k", "320k")
        /// </summary>
        public string? AudioBitrate { get; set; }

        /// <summary>
        /// Whether to use hardware acceleration if available
        /// </summary>
        public bool UseHardwareAcceleration { get; set; } = true;

        /// <summary>
        /// Whether to generate thumbnails during processing.
        /// </summary>
        public bool GenerateThumbnails { get; set; } = true;

        /// <summary>
        /// Whether to perform transcription during processing.
        /// </summary>
        public bool Transcribe { get; set; } = true;
    }

    /// <summary>
    /// Options for media joining
    /// </summary>
    public class MediaJoinOptions
    {
        /// <summary>
        /// Strategy for handling different formats
        /// </summary>
        public JoinStrategy Strategy { get; set; } = JoinStrategy.Transcode;

        /// <summary>
        /// Output format for the joined file
        /// </summary>
        public string OutputFormat { get; set; } = "mp4";

        /// <summary>
        /// Whether to add crossfades between clips
        /// </summary>
        public bool AddCrossfade { get; set; } = false;

        /// <summary>
        /// Crossfade duration in seconds (if AddCrossfade is true)
        /// </summary>
        public double CrossfadeDuration { get; set; } = 1.0;
    }

    /// <summary>
    /// Strategy for joining media files
    /// </summary>
    public enum JoinStrategy
    {
        /// <summary>
        /// Transcode all files to a common format
        /// </summary>
        Transcode,

        /// <summary>
        /// Use stream copying when possible (faster but may cause incompatibilities)
        /// </summary>
        StreamCopy,

        /// <summary>
        /// Create a playlist/container that references the original files
        /// </summary>
        Concatenate
    }

    /// <summary>
    /// Options for media conversion
    /// </summary>
    public class MediaConversionOptions
    {
        /// <summary>
        /// Target format (e.g., "mp4", "webm", "mp3")
        /// </summary>
        public string OutputFormat { get; set; } = "mp4";

        /// <summary>
        /// Quality preset (e.g., "ultrafast", "medium", "veryslow")
        /// </summary>
        public string Preset { get; set; } = "medium";

        /// <summary>
        /// Constant Rate Factor for quality control (0-51, lower is better quality)
        /// </summary>
        public int? CRF { get; set; }

        /// <summary>
        /// Video codec to use
        /// </summary>
        public string? VideoCodec { get; set; }

        /// <summary>
        /// Audio codec to use
        /// </summary>
        public string? AudioCodec { get; set; }

        /// <summary>
        /// Video bitrate (e.g., "2M", "5000k")
        /// </summary>
        public string? VideoBitrate { get; set; }

        /// <summary>
        /// Audio bitrate (e.g., "128k", "320k")
        /// </summary>
        public string? AudioBitrate { get; set; }

        /// <summary>
        /// Whether to use hardware acceleration if available
        /// </summary>
        public bool UseHardwareAcceleration { get; set; } = true;
    }

    /// <summary>
    /// Options for waveform generation
    /// </summary>
    public class WaveformOptions
    {
        /// <summary>
        /// Width of the waveform image
        /// </summary>
        public int Width { get; set; } = 1000;

        /// <summary>
        /// Height of the waveform image
        /// </summary>
        public int Height { get; set; } = 200;

        /// <summary>
        /// Background color in hex format (e.g., "#000000" for black)
        /// </summary>
        public string BackgroundColor { get; set; } = "#000000";

        /// <summary>
        /// Waveform color in hex format (e.g., "#00FF00" for green)
        /// </summary>
        public string WaveformColor { get; set; } = "#00FF00";

        /// <summary>
        /// Whether to show stereo channels separately
        /// </summary>
        public bool SplitChannels { get; set; } = false;
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Transcription; // Added for consolidated TranscriptSegment

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Represents the playback state of the media player
    /// </summary>
    public enum PlaybackState
    {
        Stopped,
        Playing,
        Paused,
        Loading,
        Error
    }

    /// <summary>
    /// Interface for media player functionality
    /// </summary>
    public interface IMediaPlayer : IDisposable
    {
        /// <summary>
        /// Gets the current playback state
        /// </summary>
        PlaybackState State { get; }

        /// <summary>
        /// Gets the current position in seconds
        /// </summary>
        double CurrentPosition { get; }

        /// <summary>
        /// Gets the total duration in seconds
        /// </summary>
        double Duration { get; }

        /// <summary>
        /// Gets or sets the audio volume (0.0 to 1.0)
        /// </summary>
        double Volume { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether audio is muted
        /// </summary>
        bool IsMuted { get; set; }

        /// <summary>
        /// Gets or sets the playback rate (e.g. 1.0 = normal, 2.0 = double speed)
        /// </summary>
        double PlaybackRate { get; set; }

        /// <summary>
        /// Gets the current video frame
        /// </summary>
        IMediaImage? CurrentFrame { get; }

        /// <summary>
        /// Gets the current transcript segment based on playback position
        /// </summary>
        TranscriptSegment? CurrentTranscriptSegment { get; }

        /// <summary>
        /// Gets information about the current media file
        /// </summary>
        MediaInfo? MediaInfo { get; }

        /// <summary>
        /// Event raised when the playback position changes
        /// </summary>
        event EventHandler<double> PositionChanged;

        /// <summary>
        /// Event raised when the playback state changes
        /// </summary>
        event EventHandler<PlaybackState> PlaybackStateChanged;

        /// <summary>
        /// Event raised when media is loaded
        /// </summary>
        event EventHandler MediaLoaded;

        /// <summary>
        /// Event raised when playback reaches the end
        /// </summary>
        event EventHandler PlaybackEnded;

        /// <summary>
        /// Event raised when an error occurs
        /// </summary>
        event EventHandler<string> ErrorOccurred;

        /// <summary>
        /// Opens a media file for playback
        /// </summary>
        /// <param name="filePath">Path to the media file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task OpenAsync(string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts or resumes playback
        /// </summary>
        Task PlayAsync();

        /// <summary>
        /// Pauses playback
        /// </summary>
        Task PauseAsync();

        /// <summary>
        /// Stops playback and resets position to beginning
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// Seeks to a specific position in the media
        /// </summary>
        /// <param name="position">Position in seconds</param>
        Task SeekAsync(double position);

        /// <summary>
        /// Steps forward by one frame
        /// </summary>
        Task StepForwardAsync();

        /// <summary>
        /// Steps backward by one frame
        /// </summary>
        Task StepBackwardAsync();

        /// <summary>
        /// Takes a snapshot of the current frame
        /// </summary>
        /// <param name="outputPath">Path to save the snapshot</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task TakeSnapshotAsync(string outputPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Extracts a clip from the media file
        /// </summary>
        /// <param name="startTime">Start time in seconds</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="outputPath">Path to save the clip</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ExtractClipAsync(double startTime, double duration, string outputPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Applies video filters to the playback
        /// </summary>
        /// <param name="filters">Dictionary of filter names and values</param>
        Task ApplyVideoFiltersAsync(Dictionary<string, float> filters);

        /// <summary>
        /// Applies audio filters to the playback
        /// </summary>
        /// <param name="filters">Dictionary of filter names and values</param>
        Task ApplyAudioFiltersAsync(Dictionary<string, float> filters);

        /// <summary>
        /// Gets the current frame as raw bytes
        /// </summary>
        Task<byte[]> GetCurrentFrameAsync();

        /// <summary>
        /// Gets the current audio samples
        /// </summary>
        Task<byte[]> GetCurrentAudioSamplesAsync();
    }
}

/// <summary>
/// Platform-agnostic image representation
/// </summary>
namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Abstraction for image data to avoid WPF dependency in Core layer
    /// </summary>
    public interface IMediaImage
    {
        /// <summary>
        /// Gets the width of the image in pixels
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the height of the image in pixels
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the raw pixel data of the image
        /// </summary>
        /// <returns>Byte array containing image data</returns>
        byte[] GetPixelData();
    }
}
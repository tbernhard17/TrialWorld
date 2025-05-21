using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Provides intelligent thumbnail generation based on content analysis.
    /// </summary>
    public interface ISmartThumbnailService
    {
        /// <summary>
        /// Gets thumbnails for key moments in a video
        /// </summary>
        /// <param name="videoPath">Path to the video file</param>
        /// <param name="count">Number of key moments to capture</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of generated thumbnails</returns>
        Task<List<SmartThumbnail>> GetKeyMomentThumbnailsAsync(
            string videoPath,
            int count = 3,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents a smart thumbnail with metadata about what it contains
    /// </summary>
    public class SmartThumbnail
    {
        /// <summary>
        /// Path to the thumbnail image
        /// </summary>
        public required string FilePath { get; set; }

        /// <summary>
        /// Timestamp in the video where the thumbnail was captured
        /// </summary>
        public TimeSpan Timestamp { get; set; }

        /// <summary>
        /// Tags associated with this thumbnail
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Confidence score for the primary feature (emotion, gesture, etc.)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Description of what's in the thumbnail
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Type of moment captured (emotion spike, gesture, key topic, etc.)
        /// </summary>
        public SmartThumbnailType ThumbnailType { get; set; }
    }

    /// <summary>
    /// Types of smart thumbnails
    /// </summary>
    public enum SmartThumbnailType
    {
        /// <summary>Emotion spike moment</summary>
        EmotionSpike,

        /// <summary>Gesture detection</summary>
        Gesture,

        /// <summary>Key topic moment</summary>
        KeyTopic,

        /// <summary>Scene change</summary>
        SceneChange,

        /// <summary>Person appearance</summary>
        PersonAppearance,

        /// <summary>Standard thumbnail</summary>
        Standard
    }
}
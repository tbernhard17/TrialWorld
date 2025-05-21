using System;
using System.Collections.Generic;

namespace TrialWorld.Core.Models
{
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
        /// Primary emotion detected (if this is an emotion thumbnail)
        /// </summary>
        public string? PrimaryEmotion { get; set; }

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
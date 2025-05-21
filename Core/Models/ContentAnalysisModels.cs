using System;
using System.Collections.Generic;

namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Represents a highlight segment identified in content.
    /// </summary>
    public class ContentHighlight
    {
        /// <summary>
        /// Gets or sets the text of the highlight.
        /// </summary>
        public required string Text { get; set; }

        /// <summary>
        /// Gets or sets the start time of the highlight in seconds.
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the highlight in seconds.
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Gets or sets the confidence score for this highlight.
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Gets or sets the type of highlight (e.g., "topic", "entity", "sentiment").
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Gets the duration of the highlight in seconds.
        /// </summary>
        public double Duration => EndTime.TotalSeconds - StartTime.TotalSeconds;
    }

    /// <summary>
    /// Represents a suggested timestamp for a thumbnail.
    /// </summary>
    public class ThumbnailSuggestion
    {
        /// <summary>
        /// Gets or sets the timestamp in seconds.
        /// </summary>
        public TimeSpan Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the confidence score for this suggestion.
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Gets or sets the reason for suggesting this timestamp.
        /// </summary>
        public required string Reason { get; set; }

        /// <summary>
        /// Gets or sets the context (surrounding text) for this timestamp.
        /// </summary>
        public required string Context { get; set; }

        /// <summary>
        /// Gets or sets additional metrics for this suggestion.
        /// </summary>
        public Dictionary<string, double> Metrics { get; set; } = new();
    }

    /// <summary>
    /// Represents a content summary with chapters and key points.
    /// </summary>
    public class ContentSummary
    {
        /// <summary>
        /// Gets or sets the overall summary text.
        /// </summary>
        public required string Summary { get; set; }

        /// <summary>
        /// Gets or sets the list of chapters in the content.
        /// </summary>
        public List<ChapterInfo> Chapters { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of key points in the content.
        /// </summary>
        public List<KeyPoint> KeyPoints { get; set; } = new();

        /// <summary>
        /// Gets or sets the total duration of the content in seconds.
        /// </summary>
        public double TotalDurationInSeconds { get; set; }

        /// <summary>
        /// Gets or sets the distribution of topics in the content.
        /// </summary>
        public Dictionary<string, double> TopicDistribution { get; set; } = new();
    }

    /// <summary>
    /// Represents a chapter in content.
    /// </summary>
    public class ChapterInfo
    {
        /// <summary>
        /// Gets or sets the title of the chapter.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Gets or sets the summary of the chapter.
        /// </summary>
        public required string Summary { get; set; }

        /// <summary>
        /// Gets or sets the start time of the chapter in seconds.
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the chapter in seconds.
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Gets the duration of the chapter in seconds.
        /// </summary>
        public double Duration => EndTime.TotalSeconds - StartTime.TotalSeconds;

        /// <summary>
        /// Gets or sets the key points in this chapter.
        /// </summary>
        public List<string> Keywords { get; set; } = new();
    }

    /// <summary>
    /// Represents a key point in the content.
    /// </summary>
    public class KeyPoint
    {
        /// <summary>
        /// Gets or sets the text of the key point.
        /// </summary>
        public required string Text { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the key point in seconds.
        /// </summary>
        public TimeSpan Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the importance of the key point.
        /// </summary>
        public double Importance { get; set; }

        /// <summary>
        /// Gets or sets related topics for this key point.
        /// </summary>
        public List<string> RelatedTopics { get; set; } = new();
    }

    /// <summary>
    /// Represents a topic or keyword extracted from content.
    /// </summary>
    public class ContentTopic
    {
        /// <summary>
        /// Gets or sets the topic name.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the confidence score for this topic.
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Gets or sets the relevance score for this topic (0.0 to 1.0).
        /// </summary>
        public double Relevance { get; set; }

        /// <summary>
        /// Gets or sets timestamps where this topic appears in seconds.
        /// </summary>
        public List<double> Timestamps { get; set; } = new List<double>();

        /// <summary>
        /// Gets or sets related keywords for this topic.
        /// </summary>
        public List<string> Keywords { get; set; } = new();

        /// <summary>
        /// Gets or sets the type of topic (e.g., "person", "organization", "concept").
        /// </summary>
        public required string Type { get; set; }

        /// <summary>
        /// Gets or sets subtopic scores for this topic.
        /// </summary>
        public Dictionary<string, double> SubtopicScores { get; set; } = new();
    }

    /// <summary>
    /// Represents a content segment identified in content.
    /// </summary>
    public class ContentSegment
    {
        /// <summary>
        /// Gets or sets the type of the segment.
        /// </summary>
        public required string Type { get; set; }

        /// <summary>
        /// Gets or sets the start time of the segment in seconds.
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the segment in seconds.
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Gets or sets the confidence score for this segment.
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Gets or sets additional features for this segment.
        /// </summary>
        public Dictionary<string, double> Features { get; set; } = new();

        /// <summary>
        /// Gets the duration of the segment in seconds.
        /// </summary>
        public double Duration => EndTime.TotalSeconds - StartTime.TotalSeconds;
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TrialWorld.Infrastructure.Services;

/// <summary>
/// User preferences for the application layer
/// </summary>
public class UserPreferences
{
    private readonly ConcurrentDictionary<string, float> _weights = new();

    /// <summary>
    /// User ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Topic weights for personalized ranking
    /// </summary>
    public Dictionary<string, double> TopicWeights { get; set; } = new Dictionary<string, double>();

    /// <summary>
    /// Speaker weights for personalized ranking
    /// </summary>
    public Dictionary<string, double> SpeakerWeights { get; set; } = new Dictionary<string, double>();

    /// <summary>
    /// Emotion weights for personalized ranking
    /// </summary>
    public Dictionary<string, double> EmotionWeights { get; set; } = new Dictionary<string, double>();

    /// <summary>
    /// List of user's favorite tags
    /// </summary>
    public List<string> FavoriteTags { get; set; } = new List<string>();

    /// <summary>
    /// Custom weights for personalized ranking
    /// </summary>
    public Dictionary<string, double> CustomWeights { get; set; } = new Dictionary<string, double>();

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    /// <summary>
    /// Total count of feedback provided by the user
    /// </summary>
    public int FeedbackCount { get; set; } = 0;

    /// <summary>
    /// Updates weights for keywords based on relevance score
    /// </summary>
    /// <param name="context">Search context</param>
    /// <param name="score">Relevance score</param>
    /// <param name="keywords">Keywords to update</param>
    public void UpdateWeights(string context, float score, string[] keywords)
    {
        const float LearningRate = 0.1f;

        foreach (var keyword in keywords)
        {
            _weights.AddOrUpdate(
                keyword,
                score,
                (_, oldScore) => oldScore + (score - oldScore) * LearningRate);
        }
    }

    /// <summary>
    /// Gets the weight for a keyword
    /// </summary>
    /// <param name="keyword">Keyword to get weight for</param>
    /// <returns>Weight value</returns>
    public float GetWeight(string keyword) =>
        _weights.GetOrAdd(keyword, _ => 1.0f);

    /// <summary>
    /// Create an instance from Core model
    /// </summary>
    public static UserPreferences FromCore(TrialWorld.Core.Models.UserPreferences core)
    {
        if (core == null) return null!;

        return new UserPreferences
        {
            UserId = core.UserId,
            TopicWeights = core.TopicWeights,
            SpeakerWeights = core.SpeakerWeights,
            EmotionWeights = core.EmotionWeights,
            FavoriteTags = core.FavoriteTags,
            CustomWeights = core.CustomWeights,
            LastUpdated = core.LastUpdated,
            FeedbackCount = core.FeedbackCount
        };
    }

    /// <summary>
    /// Convert to Core model
    /// </summary>
    public TrialWorld.Core.Models.UserPreferences ToCore()
    {
        return new TrialWorld.Core.Models.UserPreferences
        {
            UserId = UserId,
            TopicWeights = TopicWeights,
            SpeakerWeights = SpeakerWeights,
            EmotionWeights = EmotionWeights,
            FavoriteTags = FavoriteTags,
            CustomWeights = CustomWeights,
            LastUpdated = LastUpdated,
            FeedbackCount = FeedbackCount
        };
    }
}
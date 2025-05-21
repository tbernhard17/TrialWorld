using System;
using System.Collections.Generic;

namespace TrialWorld.Contracts
{
    /// <summary>
    /// Data transfer object for search result feedback.
    /// </summary>
    public class SearchResultFeedbackDto
    {
        public string MediaId { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public int? Rating { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public string ResultId { get; set; } = string.Empty;
        public double RelevanceRating { get; set; }
        public bool WasSelected { get; set; }
        public string? Comment { get; set; }
        public List<string>? UserTags { get; set; } = new List<string>();
    }
}

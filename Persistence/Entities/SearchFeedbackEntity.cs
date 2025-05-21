using System;

namespace TrialWorld.Persistence.Entities
{
    /// <summary>
    /// Placeholder entity for SearchFeedback.
    /// Define properties based on DB schema and Core.Models.SearchFeedback.
    /// </summary>
    public class SearchFeedbackEntity
    {
        public Guid Id { get; set; } // Use Guid for DB keys typically
        public string Query { get; set; } = string.Empty;
        public string Feedback { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? UserId { get; set; } // Assuming UserId exists
        // Add other properties as needed
    }
} 
using System;

namespace TrialWorld.Presentation.Models
{
    /// <summary>
    /// Defines the type of notification to display
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Information notification (neutral)
        /// </summary>
        Information,
        
        /// <summary>
        /// Success notification (positive)
        /// </summary>
        Success,
        
        /// <summary>
        /// Warning notification (caution)
        /// </summary>
        Warning,
        
        /// <summary>
        /// Error notification (negative)
        /// </summary>
        Error
    }
}

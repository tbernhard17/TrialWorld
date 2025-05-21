using System;
using System.Threading.Tasks;
using TrialWorld.Presentation.Models;

namespace TrialWorld.Presentation.Interfaces
{
    /// <summary>
    /// Interface for notification services that show toast or transient notifications
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Shows a notification with the specified message
        /// </summary>
        /// <param name="message">The notification message</param>
        /// <param name="title">Optional title for the notification</param>
        void ShowNotification(string? message, string? title = "");
        
        /// <summary>
        /// Shows a notification with the specified message and type
        /// </summary>
        /// <param name="message">The notification message</param>
        /// <param name="title">Optional title for the notification</param>
        /// <param name="type">The type of notification to display</param>
        void ShowNotification(string? message, string? title, NotificationType type);
        
        /// <summary>
        /// Shows a success notification
        /// </summary>
        /// <param name="message">The success message</param>
        /// <param name="title">Optional title for the notification</param>
        void ShowSuccess(string? message, string? title = "");
        
        /// <summary>
        /// Shows a warning notification
        /// </summary>
        /// <param name="message">The warning message</param>
        /// <param name="title">Optional title for the notification</param>
        void ShowWarning(string? message, string? title = "");
        
        /// <summary>
        /// Shows an error notification
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="title">Optional title for the notification</param>
        void ShowError(string? message, string? title = "");
    }
}

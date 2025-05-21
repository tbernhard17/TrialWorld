using TrialWorld.Presentation.Interfaces;
using TrialWorld.Presentation.Models;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace TrialWorld.Presentation.Services
{
    /// <summary>
    /// Implementation of the INotificationService interface for displaying notifications
    /// </summary>
    public class NotificationService : INotificationService
    {
        /// <summary>
        /// Shows a notification with the specified message
        /// </summary>
        /// <param name="message">The notification message</param>
        /// <param name="title">Optional title for the notification</param>
        
        
        /// <summary>
        /// Shows a notification with the specified message and type
        /// </summary>
        /// <param name="message">The notification message</param>
        /// <param name="title">Optional title for the notification</param>
        /// <param name="type">The type of notification to display</param>
        public void ShowNotification(string? message, string? title, NotificationType type)
        {
            // For now, use MessageBox for notifications
            // In a production app, this would be replaced with a custom toast notification
            MessageBoxImage icon = MessageBoxImage.Information;
            
            switch (type)
            {
                case NotificationType.Success:
                    icon = MessageBoxImage.Information; // No success icon in MessageBox
                    break;
                case NotificationType.Warning:
                    icon = MessageBoxImage.Warning;
                    break;
                case NotificationType.Error:
                    icon = MessageBoxImage.Error;
                    break;
                case NotificationType.Information:
                default:
                    icon = MessageBoxImage.Information;
                    break;
            }
            
            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }
        
        public void ShowNotification(string? message, string? title)
        {
            ShowNotification(message, title, NotificationType.Information);
        }
        
        /// <summary>
        /// Shows a success notification
        /// </summary>
        /// <param name="message">The success message</param>
        /// <param name="title">Optional title for the notification</param>
        public void ShowSuccess(string? message, string? title = "")
        {
            ShowNotification(message, title, NotificationType.Success);
        }
        
        /// <summary>
        /// Shows a warning notification
        /// </summary>
        /// <param name="message">The warning message</param>
        /// <param name="title">Optional title for the notification</param>
        public void ShowWarning(string? message, string? title = "")
        {
            ShowNotification(message, title, NotificationType.Warning);
        }
        
        /// <summary>
        /// Shows an error notification
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="title">Optional title for the notification</param>
        public void ShowError(string? message, string? title = "")
        {
            ShowNotification(message, title, NotificationType.Error);
        }
    }
}

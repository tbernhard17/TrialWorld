using System;

namespace Core
{
    /// <summary>
    /// Provides utility methods for working with events
    /// </summary>
    public static class EventManager
    {
        /// <summary>
        /// Safely raises an event with event args, checking for null handlers
        /// </summary>
        /// <typeparam name="T">Type of event args</typeparam>
        /// <param name="handler">Event handler to invoke</param>
        /// <param name="sender">Object raising the event</param>
        /// <param name="args">Event arguments</param>
        public static void RaiseEvent<T>(this EventHandler<T> handler, object sender, T args)
            where T : EventArgs
        {
            handler?.Invoke(sender, args);
        }

        /// <summary>
        /// Safely raises an event, checking for null handlers
        /// </summary>
        /// <param name="handler">Event handler to invoke</param>
        /// <param name="sender">Object raising the event</param>
        /// <param name="args">Event arguments</param>
        public static void RaiseEvent(this EventHandler handler, object sender, EventArgs args)
        {
            handler?.Invoke(sender, args);
        }

        /// <summary>
        /// Creates an event args class with progress information
        /// </summary>
        /// <param name="progress">Progress value between 0 and 100</param>
        /// <param name="message">Optional progress message</param>
        /// <returns>Progress event args</returns>
        public static ProgressEventArgs CreateProgressEventArgs(double progress, string? message = null)
        {
            return new ProgressEventArgs(progress, message);
        }
    }

    /// <summary>
    /// Event arguments for progress reporting
    /// </summary>
    public class ProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the progress percentage (0-100)
        /// </summary>
        public double Progress { get; }

        /// <summary>
        /// Gets the optional progress message
        /// </summary>
        public string? Message { get; }

        /// <summary>
        /// Gets the timestamp when this event was created
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Initializes a new instance of the ProgressEventArgs class
        /// </summary>
        /// <param name="progress">Progress value between 0 and 100</param>
        /// <param name="message">Optional progress message</param>
        public ProgressEventArgs(double progress, string? message = null)
        {
            Progress = Math.Clamp(progress, 0, 100);
            Message = message;
            Timestamp = DateTime.UtcNow;
        }
    }
}
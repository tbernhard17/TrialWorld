using System;

namespace TrialWorld.Core.Common.Interfaces
{
    /// <summary>
    /// Interface for event aggregation and publish-subscribe patterns
    /// </summary>
    public interface IEventAggregator
    {
        /// <summary>
        /// Publishes an event message to all subscribers
        /// </summary>
        /// <typeparam name="TMessage">Type of the message</typeparam>
        /// <param name="message">The message to publish</param>
        void Publish<TMessage>(TMessage message);
        
        /// <summary>
        /// Subscribes to events of a specific type
        /// </summary>
        /// <typeparam name="TMessage">Type of the message</typeparam>
        /// <param name="handler">Handler to execute when message is received</param>
        void Subscribe<TMessage>(Action<TMessage> handler);
        
        /// <summary>
        /// Unsubscribes from events
        /// </summary>
        /// <typeparam name="TMessage">Type of the message</typeparam>
        /// <param name="handler">Handler to unsubscribe</param>
        void Unsubscribe<TMessage>(Action<TMessage> handler);
    }
}

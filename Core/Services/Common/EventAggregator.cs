using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TrialWorld.Core.Common.Interfaces;

namespace TrialWorld.Core.Services.Common
{
    /// <summary>
    /// Implementation of IEventAggregator that enables loosely-coupled communication
    /// between components using a publish-subscribe pattern
    /// </summary>
    public class EventAggregator : TrialWorld.Core.Common.Interfaces.IEventAggregator
    {
        private readonly ILoggingService _logger;
        private readonly ConcurrentDictionary<Type, IList<object>> _subscribers = new ConcurrentDictionary<Type, IList<object>>();

        /// <summary>
        /// Creates a new instance of the EventAggregator
        /// </summary>
        /// <param name="logger">Logging service for diagnostic information</param>
        public EventAggregator(ILoggingService logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogDebug("EventAggregator initialized");
        }

        /// <summary>
        /// Publishes a message to all subscribers of the message type
        /// </summary>
        /// <typeparam name="TMessage">Type of the message</typeparam>
        /// <param name="message">Message to publish</param>
        public void Publish<TMessage>(TMessage message)
        {
            if (message == null)
            {
                _logger.LogWarning("Attempted to publish null message");
                return;
            }

            var messageType = typeof(TMessage);
            _logger.LogDebug($"Publishing message of type {messageType.Name}");

            if (!_subscribers.TryGetValue(messageType, out var actions))
            {
                _logger.LogDebug($"No subscribers found for message type {messageType.Name}");
                return;
            }

            // Create a snapshot of subscribers to handle potential modification during iteration
            var subscriberSnapshot = actions.ToList();
            
            foreach (var subscriber in subscriberSnapshot)
            {
                try
                {
                    var action = (Action<TMessage>)subscriber;
                    action(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error publishing message to subscriber: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// Subscribes to receive messages of a specific type
        /// </summary>
        /// <typeparam name="TMessage">Type of the message</typeparam>
        /// <param name="action">Action to execute when message is received</param>
        public void Subscribe<TMessage>(Action<TMessage> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var messageType = typeof(TMessage);
            _logger?.LogDebug($"Adding subscriber for message type {messageType.Name}");
            
            _subscribers.AddOrUpdate(
                messageType,
                _ => new List<object> { action },
                (_, list) =>
                {
                    lock (list)
                    {
                        // Prevent adding duplicate subscriptions
                        if (!list.Contains(action))
                        {
                             list.Add(action);
                        }
                        return list;
                    }
                });
        }

        /// <summary>
        /// Unsubscribes from events of a specific type.
        /// </summary>
        /// <typeparam name="TMessage">The type of message.</typeparam>
        /// <param name="action">The action to unsubscribe.</param>
        public void Unsubscribe<TMessage>(Action<TMessage> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            var messageType = typeof(TMessage);
            Unsubscribe(messageType, action);
        }

        private void Unsubscribe(Type messageType, object action)
        {
            _logger?.LogDebug($"Removing subscriber for message type {messageType.Name}");
            
            if (_subscribers.TryGetValue(messageType, out var actions))
            {
                lock (actions)
                {
                    actions.Remove(action);
                    
                    if (actions.Count == 0)
                    {
                        _subscribers.TryRemove(messageType, out _);
                    }
                }
            }
        }
    }
}

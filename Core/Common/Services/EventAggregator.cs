using System;
using System.Collections.Generic;
using TrialWorld.Core.Common.Interfaces;

namespace TrialWorld.Core.Common.Services
{
    public class EventAggregator : TrialWorld.Core.Common.Interfaces.IEventAggregator
    {
        private readonly Dictionary<Type, List<object>> _subscriptions = new Dictionary<Type, List<object>>();

        public void Publish<TEvent>(TEvent eventData)
        {
            var eventType = typeof(TEvent);
            if (!_subscriptions.ContainsKey(eventType))
            {
                return;
            }

            var handlers = _subscriptions[eventType];
            foreach (var handler in handlers.ToArray())
            {
                ((Action<TEvent>)handler)(eventData);
            }
        }

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            var eventType = typeof(TEvent);
            if (!_subscriptions.ContainsKey(eventType))
            {
                _subscriptions[eventType] = new List<object>();
            }

            if (!_subscriptions[eventType].Contains(handler))
            {
                _subscriptions[eventType].Add(handler);
            }
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            var eventType = typeof(TEvent);
            if (!_subscriptions.ContainsKey(eventType))
            {
                return;
            }

            if (_subscriptions[eventType].Contains(handler))
            {
                _subscriptions[eventType].Remove(handler);
            }
        }
    }
}

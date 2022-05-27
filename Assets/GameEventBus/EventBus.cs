using System;
using System.Collections.Generic;
using System.Linq;
using GameEventBus.Events;
using GameEventBus.Interfaces;

namespace GameEventBus
{
    /// <summary>
    /// Implements <see cref="IEventBus"/>.
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<ISubscription>> _subscriptions;
        private readonly Dictionary<object, Type> typeMap; 
        private static readonly object SubscriptionsLock = new object();

        public EventBus()
        {
            _subscriptions = new Dictionary<Type, List<ISubscription>>();
            typeMap = new Dictionary<object, Type>();
        }

        /// <summary>
        /// Subscribes to the specified event type with the specified action
        /// </summary>
        /// <typeparam name="TEventBase">The type of event</typeparam>
        /// <param name="action">The Action to invoke when an event of this type is published</param>
        /// <returns>A <see cref="SubscriptionToken"/> to be used when calling <see cref="Unsubscribe"/></returns>
        public void Subscribe<TEventBase>(Action<TEventBase> action) where TEventBase : EventBase
        {
            if (action == null)
                throw new ArgumentNullException("action");

            lock (SubscriptionsLock)
            {
                if (!_subscriptions.ContainsKey(typeof(TEventBase)))
                    _subscriptions.Add(typeof(TEventBase), new List<ISubscription>());

                typeMap.Add(action, typeof(TEventBase));
                _subscriptions[typeof(TEventBase)].Add(new Subscription<TEventBase>(action));
            }
        }

        /// <summary>
        /// Unsubscribe from the Event type related to the specified <see cref="SubscriptionToken"/>
        /// </summary>
        /// <param name="token">The <see cref="SubscriptionToken"/> received from calling the Subscribe method</param>
        public void Unsubscribe<TEventBase>(Action<TEventBase> action) where TEventBase : EventBase
        {
            if (action == null)
                throw new ArgumentNullException("action");

            lock (SubscriptionsLock)
            {
                Type type = typeMap[action];
                if (_subscriptions.ContainsKey(type))
                {
                    var allSubscriptions = _subscriptions[type];
                    var subscriptionToRemove = allSubscriptions.FirstOrDefault(x => ReferenceEquals(x.SubscriptionToken, action));
                    if (subscriptionToRemove != null)
                        _subscriptions[type].Remove(subscriptionToRemove);
                }
            }
        }

        /// <summary>
        /// Publishes the specified event to any subscribers for the <see cref="TEventBase"/> event type
        /// </summary>
        /// <typeparam name="TEventBase">The type of event</typeparam>
        /// <param name="eventItem">Event to publish</param>
        public void Publish<TEventBase>(TEventBase eventItem) where TEventBase : EventBase
        {
            if (eventItem == null)
                throw new ArgumentNullException("eventItem");

            List<ISubscription> allSubscriptions = new List<ISubscription>();
            lock (SubscriptionsLock)
            {
                if (_subscriptions.ContainsKey(typeof(TEventBase)))
                    allSubscriptions = _subscriptions[typeof(TEventBase)];
            }

            foreach (var subscription in allSubscriptions)
            {
                subscription.Publish(eventItem);
            }
        }

        /// <summary>
        /// Publishes the specified event to any subscribers for the <see cref="TEventBase"/> event type asychronously
        /// </summary>
        /// <remarks> This is a wrapper call around the synchronous  method as this method is naturally synchronous (CPU Bound) </remarks>
        /// <typeparam name="TEventBase">The type of event</typeparam>
        /// <param name="eventItem">Event to publish</param>
        public void PublishAsync<TEventBase>(TEventBase eventItem) where TEventBase : EventBase
        {
            PublishAsyncInternal(eventItem, null);
        }

        /// <summary>
        /// Publishes the specified event to any subscribers for the <see cref="TEventBase"/> event type asychronously
        /// </summary>
        /// <remarks> This is a wrapper call around the synchronous  method as this method is naturally synchronous (CPU Bound) </remarks>
        /// <typeparam name="TEventBase">The type of event</typeparam>
        /// <param name="eventItem">Event to publish</param>
        /// <param name="callback"><see cref="AsyncCallback"/> that is called on completion</param>
        public void PublishAsync<TEventBase>(TEventBase eventItem, AsyncCallback callback) where TEventBase : EventBase
        {
            PublishAsyncInternal(eventItem, callback);
        }

        #region PRIVATE METHODS

        private void PublishAsyncInternal<TEventBase>(TEventBase eventItem, AsyncCallback callback) where TEventBase : EventBase
        {
            Action publishAction = () => Publish(eventItem);
            publishAction.BeginInvoke(callback, null);
        }

        #endregion
    }
}

using System;
using System.Threading.Tasks;
using Disruptor.ReadModel.Tests.Infrastructure.FaultHandling;
using Disruptor.ReadModel.Tests.Infrastructure.Subscriptions;
using EventStore.ClientAPI;
using NLog;
using ILogger = EventStore.ClientAPI.ILogger;

namespace Disruptor.ReadModel.Tests.Infrastructure.Consumers
{
    /// <summary>
    /// The abstract base class which supports the different consumer types
    /// </summary>
    public abstract class StreamConsumer
    {
        protected readonly IEventDispatcher dispatcher;
        protected readonly IEventStoreConnection connection;
        protected Subscription subscription;
        protected readonly NLog.ILogger log;

        protected StreamConsumer(IEventStoreConnection connection, IEventDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            this.connection = connection;
            this.log = LogManager.GetLogger("StreamConsumer");
        }

        //set the default RetryPolicy for each subscription - max 5 retries with exponential backoff
        protected RetryPolicy retryPolicy = new RetryPolicy(5.Retries(), retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        public abstract Task ConnectToSubscription(Subscription subscription);

        protected async Task Dispatch(ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.Event != null && !IsSystemEvent(resolvedEvent))
            {
                await dispatcher.Dispatch(resolvedEvent);
            }
        }

        protected async Task HandleDroppedSubscription(DroppedSubscription subscriptionDropped) =>
            await DroppedSubscriptionPolicy.Handle(subscriptionDropped, async () => await ConnectToSubscription(subscription));

        private static bool IsSystemEvent(ResolvedEvent resolvedEvent) =>
            resolvedEvent.OriginalEvent.EventType.StartsWith("$");
    }
}

using System;
using System.Threading.Tasks;
using Disruptor.ReadModel.Tests.Infrastructure.Subscriptions;
using EventStore.ClientAPI;

namespace Disruptor.ReadModel.Tests.Infrastructure.Consumers
{
   
    public class ReadModelConsumer : StreamConsumer
    {
        public ReadModelConsumer(IEventStoreConnection connection, IEventDispatcher dispatcher)
                : base(connection, dispatcher) { }

        public override async Task ConnectToSubscription(Subscription subscription)
        {
            this.subscription = subscription;

            try
            {
                await Task.Run(() =>
                connection.SubscribeToAllFrom(
                    Position.Start,
                true,
                EventAppeared,
                LiveProcessingStarted,
                SubscriptionDropped));
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        private async void SubscriptionDropped(EventStoreCatchUpSubscription eventStoreCatchUpSubscription, SubscriptionDropReason subscriptionDropReason, Exception exception) =>
            await HandleDroppedSubscription(new DroppedSubscription(subscription, exception.Message, subscriptionDropReason));

        private void LiveProcessingStarted(EventStoreCatchUpSubscription eventStoreCatchUpSubscription) =>
            log.Info("Read model now caught-up");

        private async void EventAppeared(EventStoreCatchUpSubscription eventStoreCatchUpSubscription, ResolvedEvent resolvedEvent) =>
            await Dispatch(resolvedEvent);
    }
}

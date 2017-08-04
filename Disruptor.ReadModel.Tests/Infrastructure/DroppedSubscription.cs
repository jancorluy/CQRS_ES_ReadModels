using Disruptor.ReadModel.Tests.Infrastructure.FaultHandling;
using Disruptor.ReadModel.Tests.Infrastructure.Subscriptions;
using EventStore.ClientAPI;

namespace Disruptor.ReadModel.Tests.Infrastructure
{
   
    public class DroppedSubscription
    {
        public DroppedSubscription(Subscription subscription, string exceptionMessage, SubscriptionDropReason dropReason)
        {
            StreamId = subscription.StreamId;
            ExceptionMessage = exceptionMessage;
            DropReason = dropReason;
            RetryPolicy = subscription.RetryPolicy;
        }

        public string StreamId { get; }
        public string ExceptionMessage { get; }
        public SubscriptionDropReason DropReason { get; }
        public RetryPolicy RetryPolicy { get; }
    }
}

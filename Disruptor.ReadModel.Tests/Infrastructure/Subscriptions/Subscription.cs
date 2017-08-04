using System;
using Disruptor.ReadModel.Tests.Infrastructure.FaultHandling;

namespace Disruptor.ReadModel.Tests.Infrastructure.Subscriptions
{
    public abstract class Subscription
    {
        protected Subscription(string streamId, string subscriptionGroup)
        {
            StreamId = streamId;
            SubscriptionGroup = subscriptionGroup;
        }

        internal string StreamId { get; }
        internal string SubscriptionGroup { get; }
        internal RetryPolicy RetryPolicy { get; set; }

        public Subscription SetRetryPolicy(params TimeSpan[] durations)
        {
            RetryPolicy = new RetryPolicy(durations);
            return this;
        }

        public Subscription SetRetryPolicy(Retries maxNoOfRetries, Func<int, TimeSpan> provider)
        {
            RetryPolicy = new RetryPolicy(maxNoOfRetries, provider);
            return this;
        }
    }
}
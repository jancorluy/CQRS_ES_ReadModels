using System;
using Disruptor.ReadModel.Tests.Infrastructure.FaultHandling;

namespace Disruptor.ReadModel.Tests.Infrastructure.Subscriptions
{

    public class ReadModelSubscription : Subscription
    {
//        private ReadModelStorage readModelStorage;

        public ReadModelSubscription() : base(string.Empty, string.Empty) { }

        public new ReadModelSubscription SetRetryPolicy(params TimeSpan[] durations) => 
            (ReadModelSubscription) base.SetRetryPolicy(durations);

        public new ReadModelSubscription SetRetryPolicy(Retries maxNoOfRetries, Func<int, TimeSpan> provider) => 
            (ReadModelSubscription) base.SetRetryPolicy(maxNoOfRetries, provider);

    //    public Subscription WithStorage(ReadModelStorage readModelStorage)
    //    {
    //        this.readModelStorage = readModelStorage;
    //        return this;
    //    }

    //    internal ReadModelStorage Storage() => readModelStorage;
    }
}

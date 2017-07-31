using System;

namespace Disruptor.ReadModel.Tests.Messages
{
    public class OrderCreatedEvent : IEvent
    {
        public Guid OrderId { get; set; }

        public string Customer { get; set; }
    }
}
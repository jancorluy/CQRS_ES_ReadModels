using System;

namespace Disruptor.ReadModel.Tests.Messages
{
    public class OrderItemAddedToCardEvent : IEvent
    {
        public Guid OrderId { get; set; }

        public string Customer { get; set; }

        public string Card { get; set; }
    }
}
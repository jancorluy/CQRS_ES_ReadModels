using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor.ReadModel.Tests.Messages;

namespace Disruptor.ReadModel.Tests.MessageHandlers
{
    public class OrderListViewHandler : RingBufferEventHandler,
        IProjectionViewHandler<OrderCreatedEvent>,
        IProjectionViewHandler<OrderItemAddedToCardEvent>
    {
        public void Handle(OrderCreatedEvent tEvent)
        {
            Console.WriteLine($"order listview handler: order created for order {tEvent.OrderId} on thread {System.Threading.Thread.CurrentThread.ManagedThreadId}");
        }

        public void Handle(OrderItemAddedToCardEvent tEvent)
        {
            Console.WriteLine($"order listview handler: order item added for order {tEvent.OrderId} on thread {System.Threading.Thread.CurrentThread.ManagedThreadId}");
        }

        protected override bool CanProcessEvent(object dataEvent)
        {
            return (dataEvent is OrderCreatedEvent) || (dataEvent is OrderItemAddedToCardEvent);
        }
    }
}

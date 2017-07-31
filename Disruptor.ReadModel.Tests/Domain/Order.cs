using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor.ReadModel.Tests.Messages;

namespace Disruptor.ReadModel.Tests.Domain
{
    public class Order : AggregateRoot
    {
        private Guid _id;
        public override Guid Id => _id;

        public string CustomerName { get; private set; }

        public string Card { get; private set; }

        public static Order CreateOrder(Guid orderId, string customerName)
        {
            return new Order(orderId, customerName);
        }

        private Order(){}

        private Order(Guid id, string customerName): this()
        {
            var orderCreatedEvent = new OrderCreatedEvent()
            {
                Customer = customerName,
                OrderId = id
            };

            this.ApplyChange(orderCreatedEvent);
        }

        public void AddItemToCard(Guid orderId, string card)
        {
            var orderItemAddedToCardEvent = new OrderItemAddedToCardEvent()
            {
                OrderId = orderId,
                Card = card
            };
            this.ApplyChange(orderItemAddedToCardEvent);
        }

        private void Apply(OrderCreatedEvent orderCreatedEvent)
        {
            _id = orderCreatedEvent.OrderId;
            CustomerName = orderCreatedEvent.Customer;
        }

        private void Apply(OrderItemAddedToCardEvent orderItemAddedToCardEvent)
        {
            Card = orderItemAddedToCardEvent.Card;
        }
    }
}

using System;

namespace Disruptor.ReadModel.Tests.Messages
{
    public class CreateOrderCommand : ICommand
    {
        public Guid OrderId { get; set; }

        public string CustomerName { get; set; }
    }
}
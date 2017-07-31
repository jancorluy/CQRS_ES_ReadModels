using System;

namespace Disruptor.ReadModel.Tests.Messages
{
    public class AddItemToCardCommand : ICommand
    {
        public Guid OrderId { get; set; }

        public string Card { get; set; }
        
    }
}
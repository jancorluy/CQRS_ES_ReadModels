using System.Threading.Tasks;
using Disruptor.ReadModel.Tests.Infrastructure;
using Disruptor.ReadModel.Tests.Messages;

namespace Disruptor.ReadModel.Tests.Domain.CommandHandlers
{
    public class CreateOrderCommandHandler : HandlesAsync<CreateOrderCommand>
    {
        private readonly IRepository<Order> _orderRepository;

        public CreateOrderCommandHandler(IRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }


        public async Task HandleAsync(CreateOrderCommand message)
        {
            var order = Order.CreateOrder(message.OrderId, message.CustomerName);
            await _orderRepository.Save(order);
        }
    }

    public class AddItemToOrderCommandHandler : HandlesAsync<AddItemToCardCommand>
    {
        private readonly IRepository<Order> _orderRepository;

        public AddItemToOrderCommandHandler(IRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }
       
        public async Task HandleAsync(AddItemToCardCommand message)
        {
            var order = await _orderRepository.GetById(message.OrderId);
            await _orderRepository.Save(order);
        }
    }
}
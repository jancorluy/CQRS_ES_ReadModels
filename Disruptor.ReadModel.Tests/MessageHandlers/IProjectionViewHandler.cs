using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor.ReadModel.Tests.Messages;

namespace Disruptor.ReadModel.Tests.MessageHandlers
{
    public interface IProjectionViewHandler<TEvent> where TEvent : IEvent
    {
        void Handle(TEvent tEvent);
    }
}

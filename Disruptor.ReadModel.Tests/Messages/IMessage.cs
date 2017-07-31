using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor.ReadModel.Tests.Messages
{
    public interface IMessage
    {

    }

    public interface IEvent : IMessage
    {
    }

    public interface ICommand : IMessage
    {
    }

}

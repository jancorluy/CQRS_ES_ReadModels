using System;
using Disruptor.ReadModel.Tests.Infrastructure;

namespace Disruptor.ReadModel.Tests.MessageHandlers
{
    public abstract class RingBufferEventHandler : IEventHandler<ReadModelEventStream>
    {
        public void OnEvent(ReadModelEventStream data, long sequence, bool endOfBatch)
        {
            if (this.CanProcessEvent(data.Event))
            {
                dynamic handler = Activator.CreateInstance(this.GetType());
                dynamic @event = data.Event;
                if (!handler.IsThisMessageAlreadyHandled(data))
                {
                    handler.Handle(@event);
                    handler.StorePosition(data);
                }
            }
        }

        private void StorePosition(ReadModelEventStream data)
        {
            //todo store the position
            Console.WriteLine($"position stored commit: {data.CommitPosition}, prepare: {data.PreparePosition}");
        }

        private bool IsThisMessageAlreadyHandled(ReadModelEventStream data)
        {
            //todo check stored position
            Console.WriteLine("checking positions");
            return false;
        }

        protected abstract bool CanProcessEvent(object dataEvent);
    }
}
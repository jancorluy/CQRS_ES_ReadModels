using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Disruptor.Dsl;
using Disruptor.ReadModel.Tests.MessageHandlers;

namespace Disruptor.ReadModel.Tests.Infrastructure
{
    public class ReadModelEventStream
    {
        public object Event { get; set; }

        public long CommitPosition { get; set; }

        public long PreparePosition { get; set; }
    }

    public static class ReadmodelHandlerExtensions
    {
        public static IEnumerable<IEventHandler<ReadModelEventStream>> GetRingBufferEventHandlersFromAssembly(this Assembly assembly)
        {
            var ringBufferEventHandlerTypes = assembly.GetTypes().Where(t => t.BaseType == typeof(RingBufferEventHandler));

            foreach (var ringBufferEventHandlerType in ringBufferEventHandlerTypes)
            {
                yield return Activator.CreateInstance(ringBufferEventHandlerType) as RingBufferEventHandler;
            }
        }
    }

    public class ReadmodelPublisher
    {
        private int ringbufferSize = (int) Math.Pow(128, 2);

        private RingBuffer<ReadModelEventStream> ringBuffer;
        private Disruptor<ReadModelEventStream> disruptor;

        private long next;
        private int sequence = 0;

        public void Start(Assembly domainEventsAssembly)
        {

            disruptor = new Disruptor<ReadModelEventStream>(() => new ReadModelEventStream(), ringbufferSize,
                TaskScheduler.Current, ProducerType.Multi, new BusySpinWaitStrategy());

            var ringBufferEventHandlers = domainEventsAssembly.GetRingBufferEventHandlersFromAssembly();
            disruptor.HandleEventsWith(ringBufferEventHandlers.ToArray());
    
            ringBuffer = disruptor.Start();
        }

        public void Stop()
        {
            disruptor.Halt();
        }

        public void Send<TEvent>(TEvent @event, long commitPosition, long preparePosition)
        {
            var eventPublisher = new EventPublisher<ReadModelEventStream>(ringBuffer);

            eventPublisher.PublishEvent((entry, pos) =>
            {
                entry.CommitPosition = commitPosition;
                entry.PreparePosition = preparePosition;
                entry.Event = @event;
                return entry;
            });
        }
    }
}
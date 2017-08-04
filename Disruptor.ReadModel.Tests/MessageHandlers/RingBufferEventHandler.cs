using System;
using Disruptor.ReadModel.Tests.Infrastructure;
using Disruptor.ReadModel.Tests.Infrastructure.Repositories;
using NLog;


namespace Disruptor.ReadModel.Tests.MessageHandlers
{
    public abstract class RingBufferEventHandler : IEventHandler<ReadModelEventStream>
    {
        private RedisReadModelRepository _repo = new RedisReadModelRepository();
        private ILogger _logger = LogManager.GetCurrentClassLogger(typeof(RingBufferEventHandler));

        public void OnEvent(ReadModelEventStream data, long sequence, bool endOfBatch)
        {
            if (this.CanProcessEvent(data.Event))
            {
                dynamic handler = Activator.CreateInstance(this.GetType());

                dynamic @event = data.Event;

                try
                {
                    if (!handler.IsThisMessageAlreadyHandled(data))
                    {
                        handler.Handle(@event);
                    }

                    handler.StorePosition(data);
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                }
            }

        }

        private void StorePosition(ReadModelEventStream data)
        {
            using (var lockToken = _repo.TakeLock(this.GetType().FullName))
            {
                var readModelHandler = _repo.GetReadModelHandlerByType(this.GetType().FullName).Result;
                if (readModelHandler == null)
                {
                    readModelHandler = new ReadModelHandler()
                    {
                        CommitPosition = data.CommitPosition,
                        PreparePosition = data.PreparePosition,
                        ReadmodelType = this.GetType().FullName,
                        LastComittedPosition = DateTime.Now
                    };

                    _repo.Add(readModelHandler);

                }
                else
                {
                    readModelHandler.CommitPosition = data.CommitPosition;
                    readModelHandler.PreparePosition = data.PreparePosition;
                    readModelHandler.LastComittedPosition = DateTime.Now;

                    _repo.Update(readModelHandler);
                }
            }

            _logger.Trace($"position stored commit: {data.CommitPosition}, prepare: {data.PreparePosition}");
        }

        private bool IsThisMessageAlreadyHandled(ReadModelEventStream data)
        {
            using (var lockToken = _repo.TakeLock(this.GetType().FullName))
            {
                var readModelHandler = _repo.GetReadModelHandlerByType(this.GetType().FullName).Result;
                if (readModelHandler == null)
                    return false;

                _logger.Trace(
                    $"checking positions: data position {data.CommitPosition} readmodelhandler position: {readModelHandler.CommitPosition}");
                if (readModelHandler.CommitPosition < data.CommitPosition)
                    return false;
                else
                {
                    return true;
                }
            }
        }

        protected abstract bool CanProcessEvent(object dataEvent);
    }
}
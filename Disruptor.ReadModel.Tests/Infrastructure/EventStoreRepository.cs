using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor.ReadModel.Tests.Domain;
using Disruptor.ReadModel.Tests.Extensions;
using Disruptor.ReadModel.Tests.Messages;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Disruptor.ReadModel.Tests.Infrastructure
{
    public interface IEventStore
    {
        Task SaveEvents<TAggregate>(Guid id, IEnumerable<IEvent> changes,int originalVersion, int expectedVersion)
            where TAggregate: AggregateRoot;

        Task<IEnumerable<IEvent>> GetEventsForAggregate<TAggregate>(Guid id)
            where TAggregate: AggregateRoot;
    }

    public class EventStore : IEventStore
    {
        private readonly IEventStoreConnection _connection;
        private readonly ReadmodelPublisher _publisher;


        public EventStore(IEventStoreConnection connection,
            ReadmodelPublisher publisher)
        {
            _connection = connection;
            _publisher = publisher;
        }

        public async Task SaveEvents<TAggregate>(Guid id, IEnumerable<IEvent> changes, int originalVersion, int expectedVersion) where TAggregate : AggregateRoot
        {
            var commitId = Guid.NewGuid();
            var events = changes.ToArray();
            if (events.Any() == false)
                return;
            var streamName = GetStreamName(typeof(TAggregate), id);
            var commitHeaders = new Dictionary<string, object>
            {
                {"CommitId", commitId},
                {"AggregateClrType", typeof(TAggregate).AssemblyQualifiedName}
            };
            var eventsToSave = events.Select(e => e.ToEventData(commitHeaders)).ToList();
            var result = await _connection.AppendToStreamAsync(streamName, expectedVersion, eventsToSave);
            
            foreach (var @event in events)
            {
                _publisher.Send(@event, result.LogPosition.CommitPosition, result.LogPosition.PreparePosition);
            }
        }

        public async Task<IEnumerable<IEvent>> GetEventsForAggregate<TAggregate>(Guid id) where TAggregate : AggregateRoot
        {

            var events = new List<IEvent>();
            StreamEventsSlice currentSlice;
            var nextSliceStart = StreamPosition.Start;
            var streamName = GetStreamName<TAggregate>(id);
            do
            {
                currentSlice = await _connection
                    .ReadStreamEventsForwardAsync(streamName, nextSliceStart, 200, false);
                nextSliceStart = (int)currentSlice.NextEventNumber;
                events.AddRange(currentSlice.Events.Select(x => x.DeserializeEvent()));
            } while (!currentSlice.IsEndOfStream);
            return events;
        }

        private string GetStreamName<T>(Guid id)
        {
            return GetStreamName(typeof(T), id);
        }

        private string GetStreamName(Type type, Guid id)
        {
            return $"{type.FullName}-{id}";
        }


    }

    public interface IRepository<T> where T : AggregateRoot
    {
        Task Save(AggregateRoot aggregate);
        Task<T> GetById(Guid id);
    }

    public class Repository<T> : IRepository<T> where T : AggregateRoot
    {
        private readonly IEventStore _storage;

        public Repository(IEventStore storage)
        {
            _storage = storage;
        }

        public async Task Save(AggregateRoot aggregate)
        {
            var originalVersion = aggregate.Version - aggregate.GetUncommittedChanges().Count();
            var expectedVersion = originalVersion == 0 ? ExpectedVersion.NoStream : originalVersion - 1;

            await _storage.SaveEvents<T>(aggregate.Id, aggregate.GetUncommittedChanges(), originalVersion, expectedVersion);
            aggregate.MarkChangesAsCommitted();
        }

        public async Task<T> GetById(Guid id)
        {
            var obj = Activator.CreateInstance(typeof(T), true) as T;
            var e = await _storage.GetEventsForAggregate<T>(id);
            obj.LoadsFromHistory(e);
            return obj;
        }
    }
}
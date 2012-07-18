using System;
using System.Linq;
using CQRSlite.Domain.Exception;
using CQRSlite.Eventing;
using CQRSlite.Infrastructure;

namespace CQRSlite.Domain
{
    public class Repository : IRepository
    {
        private readonly IEventStore _eventStore;
        private readonly IEventPublisher _publisher;

        public Repository(IEventStore eventStore, IEventPublisher publisher)
        {
            _eventStore = eventStore;
            _publisher = publisher;
        }

        public void Save<T>(T aggregate) where T : AggregateRoot
        {
            var i = 0;
            foreach (var @event in aggregate.GetUncommittedChanges())
            {
                i++;
                @event.Version = aggregate.Version + i;
                @event.TimeStamp = DateTimeOffset.UtcNow;
                _eventStore.Save(aggregate.Id, @event);
                _publisher.Publish(@event);
            }
            aggregate.MarkChangesAsCommitted();
        }

        public T Get<T>(Guid aggregateId, int? expectedVersion = null) where T : AggregateRoot
        {
            if (expectedVersion != null && _eventStore.GetVersion(aggregateId) != expectedVersion && expectedVersion != -1)
                throw new ConcurrencyException();
            return LoadAggregate<T>(aggregateId);
        }

        public int GetVersion(Guid aggregateId)
        {
            return _eventStore.GetVersion(aggregateId);
        }

        private T LoadAggregate<T>(Guid id) where T : AggregateRoot
        {
            var aggregate = AggregateActivator.CreateAggregate<T>();

            var events = _eventStore.Get(id, -1);
            if (!events.Any())
                throw new AggregateNotFoundException();

            aggregate.LoadFromHistory(events);
            return aggregate;
        }
    }
}
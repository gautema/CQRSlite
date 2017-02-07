using CQRSlite.Domain.Exception;
using CQRSlite.Domain.Factories;
using CQRSlite.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQRSlite.Domain
{
    public class Repository : IRepository
    {
        private readonly IEventStore _eventStore;
        private readonly IEventPublisher _publisher;

        public Repository(IEventStore eventStore)
        {
            if (eventStore == null)
            {
                throw new ArgumentNullException(nameof(eventStore));
            }

            _eventStore = eventStore;
        }

        [Obsolete("The eventstore should publish events after saving")]
        public Repository(IEventStore eventStore, IEventPublisher publisher)
        {
            if (eventStore == null)
            {
                throw new ArgumentNullException(nameof(eventStore));
            }
            if (publisher == null)
            {
                throw new ArgumentNullException(nameof(publisher));
            }
            _eventStore = eventStore;
            _publisher = publisher;
        }

        public void Save<T>(T aggregate, int? expectedVersion = null) where T : AggregateRoot
        {
            if (expectedVersion != null && _eventStore.Get<T>(aggregate.Id, expectedVersion.Value).Any())
            {
                throw new ConcurrencyException(aggregate.Id);
            }

            var changes = aggregate.FlushUncommitedChanges();
            _eventStore.Save<T>(changes);

            PublishChanges(changes);
        }

        public async Task SaveAsync<T>(T aggregate, int? expectedVersion = default(int?)) where T : AggregateRoot
        {
            if (expectedVersion != null && _eventStore.Get<T>(aggregate.Id, expectedVersion.Value).Any())
            {
                throw new ConcurrencyException(aggregate.Id);
            }

            var changes = aggregate.FlushUncommitedChanges();
            await _eventStore.SaveAsync<T>(changes);

            await PublishChangesAsync(changes);
        }

        private void PublishChanges(System.Collections.Generic.IEnumerable<IEvent> changes)
        {
            if (_publisher != null)
            {
                foreach (var @event in changes)
                {
                    _publisher.Publish(@event);
                }
            }
        }

        private async Task PublishChangesAsync(System.Collections.Generic.IEnumerable<IEvent> changes)
        {
            if (_publisher != null)
            {
                foreach (var @event in changes)
                {
                    await _publisher.PublishAsync(@event);
                }
            }
        }

        public T Get<T>(Guid aggregateId) where T : AggregateRoot
        {
            var events = _eventStore.Get<T>(aggregateId, -1);
            return LoadAggregate<T>(aggregateId, events);
        }

        public async Task<T> GetAsync<T>(Guid aggregateId) where T : AggregateRoot
        {
            var events = await _eventStore.GetAsync<T>(aggregateId, -1);
            return LoadAggregate<T>(aggregateId, events);
        }

        private T LoadAggregate<T>(Guid id, IEnumerable<IEvent> events) where T : AggregateRoot
        {
            if (!events.Any())
            {
                throw new AggregateNotFoundException(typeof(T), id);
            }

            var aggregate = AggregateFactory.CreateAggregate<T>();
            aggregate.LoadFromHistory(events);
            return aggregate;
        }
    }
}

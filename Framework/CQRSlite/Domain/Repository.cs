using CQRSlite.Domain.Exception;
using CQRSlite.Domain.Factories;
using CQRSlite.Events;
using System;
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

        public async Task Save<T>(T aggregate, int? expectedVersion = null) where T : AggregateRoot
        {
            if (expectedVersion != null && (await _eventStore.Get<T>(aggregate.Id, expectedVersion.Value)).Any())
            {
                throw new ConcurrencyException(aggregate.Id);
            }

            var changes = aggregate.FlushUncommitedChanges();
            await _eventStore.Save<T>(changes);

            if (_publisher != null)
            {
                foreach (var @event in changes)
                {
                    await _publisher.Publish(@event);
                }
            }
        }

        public Task<T> Get<T>(Guid aggregateId) where T : AggregateRoot
        {
            return LoadAggregate<T>(aggregateId);
        }

        private async Task<T> LoadAggregate<T>(Guid id) where T : AggregateRoot
        {
            var events = await _eventStore.Get<T>(id, -1);
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

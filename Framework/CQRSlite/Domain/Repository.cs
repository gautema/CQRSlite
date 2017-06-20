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
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        }

        [Obsolete("The eventstore should publish events after saving")]
        public Repository(IEventStore eventStore, IEventPublisher publisher)
        {
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        public async Task Save<T>(T aggregate, int? expectedVersion = null) where T : AggregateRoot
        {
            if (expectedVersion != null && (await _eventStore.Get(aggregate.Id, expectedVersion.Value)).Any())
            {
                throw new ConcurrencyException(aggregate.Id);
            }

            var changes = aggregate.FlushUncommitedChanges();
            await _eventStore.Save(changes);

            if (_publisher != null)
            {
                foreach (var @event in changes)
                {
                    await _publisher.Publish(@event);
                }
            }
        }

        public Task<T> Get<T>(IIdentity aggregateId) where T : AggregateRoot
        {
            return LoadAggregate<T>(aggregateId);
        }

        private async Task<T> LoadAggregate<T>(IIdentity id) where T : AggregateRoot
        {
            var events = await _eventStore.Get(id, -1);
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

using CQRSlite.Domain.Exception;
using CQRSlite.Domain.Factories;
using CQRSlite.Events;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSlite.Domain
{
    /// <summary>
    /// Repository gets and saves aggregates from event store.
    /// </summary>
    public class Repository : IRepository
    {
        private readonly IEventStore _eventStore;
        private readonly IEventPublisher _publisher;

        /// <summary>
        /// Initialize Repository
        /// </summary>
        /// <param name="eventStore">EventStore to get events from</param>
        public Repository(IEventStore eventStore)
        {
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        }

        /// <summary>
        /// Initialize Repository with publisher that sends all saved events to handlers.
        /// This should be done from EventStore to better handle transaction boundaries
        /// </summary>
        /// <param name="eventStore"></param>
        /// <param name="publisher"></param>
        [Obsolete("The eventstore should publish events after saving")]
        public Repository(IEventStore eventStore, IEventPublisher publisher)
        {
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        public async Task Save<T>(T aggregate, int? expectedVersion = null, CancellationToken cancellationToken = default(CancellationToken)) where T : AggregateRoot
        {
            if (expectedVersion != null && (await _eventStore.Get(aggregate.Id, expectedVersion.Value, cancellationToken)).Any())
            {
                throw new ConcurrencyException(aggregate.Id);
            }

            var changes = aggregate.FlushUncommitedChanges();
            await _eventStore.Save(changes, cancellationToken);

            if (_publisher != null)
            {
                foreach (var @event in changes)
                {
                    await _publisher.Publish(@event, cancellationToken);
                }
            }
        }

        public Task<T> Get<T>(Guid aggregateId, CancellationToken cancellationToken = default(CancellationToken)) where T : AggregateRoot
        {
            return LoadAggregate<T>(aggregateId, cancellationToken);
        }

        private async Task<T> LoadAggregate<T>(Guid id, CancellationToken cancellationToken = default(CancellationToken)) where T : AggregateRoot
        {
            var events = await _eventStore.Get(id, -1, cancellationToken);
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

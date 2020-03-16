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

        public async Task Save<T>(T aggregate, int? expectedVersion = null, CancellationToken cancellationToken = default) where T : AggregateRoot
        {
            if (expectedVersion != null && (await _eventStore.Get(aggregate.Id, expectedVersion.Value, cancellationToken).ConfigureAwait(false)).Any())
            {
                throw new ConcurrencyException(aggregate.Id);
            }
            else if(expectedVersion == null)
            {
                var savedChanges = await _eventStore.Get(aggregate.Id, aggregate.Version, cancellationToken).ConfigureAwait(false);
                aggregate.LoadFromHistory(savedChanges);
            }

            var changes = aggregate.FlushUncommittedChanges();
            await _eventStore.Save(changes, cancellationToken).ConfigureAwait(false);

            if (_publisher != null)
            {
                foreach (var @event in changes)
                {
                    await _publisher.Publish(@event, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public Task<T> Get<T>(Guid aggregateId, CancellationToken cancellationToken = default) where T : AggregateRoot
        {
            return LoadAggregate<T>(aggregateId, cancellationToken);
        }

        private async Task<T> LoadAggregate<T>(Guid id, CancellationToken cancellationToken = default) where T : AggregateRoot
        {
            var events = await _eventStore.Get(id, -1, cancellationToken).ConfigureAwait(false);
            if (!events.Any())
            {
                throw new AggregateNotFoundException(typeof(T), id);
            }

            var aggregate = AggregateFactory<T>.CreateAggregate();
            aggregate.LoadFromHistory(events);
            return aggregate;
        }
    }
}

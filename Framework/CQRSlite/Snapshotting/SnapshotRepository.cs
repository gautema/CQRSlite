using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Domain;
using CQRSlite.Domain.Factories;
using CQRSlite.Events;
using CQRSlite.Infrastructure;

namespace CQRSlite.Snapshotting
{
    /// <summary>
    /// Repository decorator that can snapshot aggregates.
    /// </summary>
    public class SnapshotRepository : IRepository
    {
        private readonly ISnapshotStore _snapshotStore;
        private readonly ISnapshotStrategy _snapshotStrategy;
        private readonly IRepository _repository;
        private readonly IEventStore _eventStore;

        /// <summary>
        /// Initialize a new instance of SnapshotRepository
        /// </summary>
        /// <param name="snapshotStore">ISnapshotStore snapshots should be saved to and fetched from</param>
        /// <param name="snapshotStrategy">ISnapshotStrategy on when to take and if to restore from snapshot</param>
        /// <param name="repository">Reposiory that gets aggregate from event store</param>
        /// <param name="eventStore">Event store where events after snapshot can be fetched from</param>
        public SnapshotRepository(ISnapshotStore snapshotStore, ISnapshotStrategy snapshotStrategy, IRepository repository, IEventStore eventStore)
        {
            _snapshotStore = snapshotStore ?? throw new ArgumentNullException(nameof(snapshotStore));
            _snapshotStrategy = snapshotStrategy ?? throw new ArgumentNullException(nameof(snapshotStrategy));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        }

        public Task Save<T>(T aggregate, int? exectedVersion = null, CancellationToken cancellationToken = default(CancellationToken)) where T : AggregateRoot
        {
            return Task.WhenAll(TryMakeSnapshot(aggregate), _repository.Save(aggregate, exectedVersion, cancellationToken));
        }

        public async Task<T> Get<T>(Guid aggregateId, CancellationToken cancellationToken = default(CancellationToken)) where T : AggregateRoot
        {
            var aggregate = AggregateFactory.CreateAggregate<T>();
            var snapshotVersion = await TryRestoreAggregateFromSnapshot(aggregateId, aggregate, cancellationToken);
            if (snapshotVersion == -1)
                return await _repository.Get<T>(aggregateId, cancellationToken);

            var events = (await _eventStore.Get(aggregateId, snapshotVersion, cancellationToken)).Where(desc => desc.Version > snapshotVersion);
            aggregate.LoadFromHistory(events);

            return aggregate;
        }

        private async Task<int> TryRestoreAggregateFromSnapshot<T>(Guid id, T aggregate, CancellationToken cancellationToken) where T : AggregateRoot
        {
            if (!_snapshotStrategy.IsSnapshotable(typeof(T)))
                return -1;
            var snapshot = await _snapshotStore.Get(id, cancellationToken);
            if (snapshot == null)
                return -1;
            aggregate.Invoke("Restore", snapshot);
            return snapshot.Version;
        }

        private Task TryMakeSnapshot(AggregateRoot aggregate)
        {
            if (!_snapshotStrategy.ShouldMakeSnapShot(aggregate))
                return Task.FromResult(0);

            dynamic snapshot = aggregate.Invoke("GetSnapshot");
            snapshot.Version = aggregate.Version + aggregate.GetUncommittedChanges().Length;
            return _snapshotStore.Save(snapshot);
        }
    }
}
using CQRSlite.Domain;
using CQRSlite.Domain.Factories;
using CQRSlite.Events;
using CQRSlite.Infrastructure;

namespace CQRSlite.Snapshotting;

/// <inheritdoc />
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
    /// <param name="repository">Repository that gets aggregate from event store</param>
    /// <param name="eventStore">Event store where events after snapshot can be fetched from</param>
    public SnapshotRepository(ISnapshotStore snapshotStore, ISnapshotStrategy snapshotStrategy, IRepository repository, IEventStore eventStore)
    {
        _snapshotStore = snapshotStore ?? throw new ArgumentNullException(nameof(snapshotStore));
        _snapshotStrategy = snapshotStrategy ?? throw new ArgumentNullException(nameof(snapshotStrategy));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
    }

    public async Task Save<T>(T aggregate, int? expectedVersion = null, CancellationToken cancellationToken = default) where T : AggregateRoot
    {
        // Decide before saving, since the strategy looks at the uncommitted changes
        var makeSnapshot = _snapshotStrategy.ShouldMakeSnapShot(aggregate);
        await _repository.Save(aggregate, expectedVersion, cancellationToken).ConfigureAwait(false);

        // Snapshot only once the events are stored, and from the state after saving, which can include
        // events from other writers that the save loaded first
        if (makeSnapshot)
        {
            var snapshot = (Snapshot)aggregate.Invoke("GetSnapshot")!;
            snapshot.Version = aggregate.Version;
            await _snapshotStore.Save(snapshot, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<T> Get<T>(Guid aggregateId, CancellationToken cancellationToken = default) where T : AggregateRoot
    {
        var aggregate = AggregateFactory<T>.CreateAggregate();
        var snapshotVersion = await TryRestoreAggregateFromSnapshot(aggregateId, aggregate, cancellationToken).ConfigureAwait(false);
        if (snapshotVersion == -1)
            return await _repository.Get<T>(aggregateId, cancellationToken).ConfigureAwait(false);

        var events = (await _eventStore.Get(aggregateId, snapshotVersion, cancellationToken).ConfigureAwait(false))
            .Where(desc => desc.Version > snapshotVersion);
        aggregate.LoadFromHistory(events);

        return aggregate;
    }

    private async Task<int> TryRestoreAggregateFromSnapshot<T>(Guid id, T aggregate, CancellationToken cancellationToken) where T : AggregateRoot
    {
        if (!_snapshotStrategy.IsSnapshotable(typeof(T)))
            return -1;
        var snapshot = await _snapshotStore.Get(id, cancellationToken).ConfigureAwait(false);
        if (snapshot == null)
            return -1;
        aggregate.Invoke("Restore", snapshot);
        return snapshot.Version;
    }
}
# Snapshots

Read this when an aggregate has so many events that loading it has become slow. A snapshot stores an aggregate's state at some version, so loading only replays the events after it. Most aggregates never need one: replaying a few hundred events is fast. Measure first.

Snapshots are only an optimization. The events stay the source of truth, and a snapshot can always be thrown away and rebuilt from them.

## Making an aggregate snapshotable

Inherit `SnapshotAggregateRoot<TSnapshot>` instead of `AggregateRoot`, and write a snapshot class holding the aggregate's state. Here is the sample's `InventoryItem` with snapshots:

```csharp
public class InventoryItemSnapshot : Snapshot
{
    public bool Activated { get; set; }
    public int Count { get; set; }
}

public class InventoryItem : SnapshotAggregateRoot<InventoryItemSnapshot>
{
    private bool _activated;
    private int _count;

    // Constructors, Apply methods and behaviour as before

    protected override InventoryItemSnapshot CreateSnapshot()
    {
        // Id and Version are set by the framework
        return new InventoryItemSnapshot { Activated = _activated, Count = _count };
    }

    protected override void RestoreFromSnapshot(InventoryItemSnapshot snapshot)
    {
        _activated = snapshot.Activated;
        _count = snapshot.Count;
    }
}
```

`CreateSnapshot` copies every field that `Apply` methods set. `RestoreFromSnapshot` puts them back. Don't set `Id` or `Version` in either one: the framework sets them on the snapshot when saving, and on the aggregate when restoring.

## Storing snapshots

Implement `ISnapshotStore`. It only ever needs the latest snapshot of each aggregate, so store one row per aggregate and overwrite it. `Get` returns `null` when there is no snapshot:

```csharp
// Keeps only the latest snapshot per aggregate, like a real store should
public class InMemorySnapshotStore : ISnapshotStore
{
    private readonly ConcurrentDictionary<Guid, Snapshot> _snapshots = new();

    public Task<Snapshot?> Get(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_snapshots.TryGetValue(id, out var snapshot) ? snapshot : null);
    }

    public Task Save(Snapshot snapshot, CancellationToken cancellationToken = default)
    {
        _snapshots[snapshot.Id] = snapshot;
        return Task.CompletedTask;
    }
}
```

A database-backed store serializes the snapshot with a type name, just like an event store does (see [Storing events](event-store.md#storing-events)). If a stored snapshot can't be read, for example because the class changed, have `Get` return `null`: the aggregate is then loaded from its events, and the next snapshot replaces the bad one.

## Wiring it up

`SnapshotRepository` wraps the normal repository:

```csharp
builder.Services.AddSingleton<ISnapshotStore, InMemorySnapshotStore>();
builder.Services.AddSingleton<ISnapshotStrategy, DefaultSnapshotStrategy>();
builder.Services.AddScoped<IRepository>(sp =>
{
    var eventStore = sp.GetRequiredService<IEventStore>();
    return new SnapshotRepository(
        sp.GetRequiredService<ISnapshotStore>(),
        sp.GetRequiredService<ISnapshotStrategy>(),
        new Repository(eventStore),
        eventStore);
});
```

Aggregates that aren't snapshotable go through the inner repository as before, so you can wrap everything.

To use snapshots together with caching, see [Caching](caching.md#with-snapshots).

## What happens when loading and saving

**Loading** a snapshotable aggregate: `SnapshotRepository` asks the snapshot store for the latest snapshot. If there is one, it restores the aggregate from it and replays only the events after the snapshot's version. If there is none, it loads all events through the inner repository.

**Saving**: before saving, the strategy decides whether a snapshot is due. Then the events are saved through the inner repository. Only if that succeeds is a snapshot taken from the aggregate's state after the save and stored, with the snapshot's version set to the aggregate's new version. If saving the events fails, for example with a `ConcurrencyException`, no snapshot is stored.

## When snapshots are taken

`DefaultSnapshotStrategy` takes a snapshot every 100 events, or every `interval` events with `new DefaultSnapshotStrategy(interval)`. The interval is a `ushort`, and 0 throws `ArgumentOutOfRangeException`.

More precisely: a snapshot is taken when the save moves the aggregate's version past a multiple of the interval. An aggregate at version 98 that saves 5 events (99 to 103) gets one snapshot, at 103.

Write your own `ISnapshotStrategy` for a different rule. `ShouldMakeSnapShot` is called before saving, so the new events are still in `GetUncommittedChanges()`:

<!-- snippet: Framework/CQRSlite/Snapshotting/DefaultSnapshotStrategy.cs -->
```csharp
public bool ShouldMakeSnapShot(AggregateRoot aggregate)
{
    if (!IsSnapshotable(aggregate.GetType()))
        return false;

    var i = aggregate.Version;
    for (var j = 0; j < aggregate.GetUncommittedChanges().Length; j++)
        if (++i % _snapshotInterval == 0 && i != 0)
            return true;
    return false;
}
```

`IsSnapshotable` must only return `true` for classes derived from `SnapshotAggregateRoot<>`, because `SnapshotRepository` calls their `GetSnapshot` and `Restore` methods. The simplest way is to reuse the default check:

```csharp
public class FirstAt50ThenEvery100 : ISnapshotStrategy
{
    private readonly DefaultSnapshotStrategy _default = new();

    // SnapshotRepository calls GetSnapshot and Restore, so only SnapshotAggregateRoot types qualify.
    // Reuse the default check rather than writing your own.
    public bool IsSnapshotable(Type aggregateType) => _default.IsSnapshotable(aggregateType);

    public bool ShouldMakeSnapShot(AggregateRoot aggregate)
    {
        if (!IsSnapshotable(aggregate.GetType()))
            return false;

        // Called before saving, so the new events are still uncommitted
        var from = aggregate.Version;
        var to = from + aggregate.GetUncommittedChanges().Length;
        return (from < 50 && to >= 50) || to / 100 > from / 100;
    }
}
```

## Changing an aggregate's state

When you add a field to an aggregate, existing snapshots don't have it. Restoring from one would leave the field empty, even though the events would have set it.

Don't migrate snapshots. Delete them: they are rebuilt from the events the next time each aggregate passes a snapshot interval, and until then aggregates load from their events. If deleting them all at once is too slow, keep a version number in your snapshot class and have the store's `Get` return `null` for older versions.

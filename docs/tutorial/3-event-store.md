# Tutorial 3: The event store

When a command handler commits, the session asks the repository to save each aggregate, and the repository hands the aggregate's new events to the event store. This page looks at that last step, using the sample's in-memory store. The [event store guide](../guides/event-store.md) covers building a real one.

## What the repository does

You don't usually call the repository yourself, but it helps to know what it does, because it is what the event store has to support.

**Loading** an aggregate means asking the event store for all of its events, creating an empty aggregate with its parameterless constructor, and replaying the events into it, oldest first. If there are no events, the aggregate doesn't exist and you get an `AggregateNotFoundException`.

**Saving** an aggregate means three things:

1. Checking that nobody else has stored events for it since it was loaded. The session passes the version the aggregate had when it was loaded; if the event store has any events after that version, the repository throws a `ConcurrencyException`.
2. Taking the aggregate's uncommitted events and stamping them: each gets the aggregate's id if it has none, its version number, and the current time.
3. Passing them to the event store's `Save`.

## The contract

`IEventStore` has two methods:

```csharp
public interface IEventStore
{
    Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default);
    Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default);
}
```

`Get` returns an aggregate's events with a version greater than `fromVersion`, in version order. The repository passes `-1` to get the whole history.

`Save` stores the events it is given. It has two jobs besides storing, and both matter.

## The sample's store

Here is the sample's implementation of `Save`:

<!-- snippet: Sample/CQRSCode/WriteModel/InMemoryEventStore.cs -->
```csharp
public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
{
    var newEvents = events.ToArray();
    lock (_inMemoryDb)
    {
        // Check everything before storing anything, so a refused save stores nothing
        foreach (var aggregateEvents in newEvents.GroupBy(e => e.Id))
        {
            var storedCount = _inMemoryDb.TryGetValue(aggregateEvents.Key, out var stored) ? stored.Count : 0;
            if (aggregateEvents.First().Version != storedCount + 1)
            {
                throw new ConcurrencyException(aggregateEvents.Key);
            }
        }

        // ...
    }

    // Publish only once the events are stored
    foreach (var @event in newEvents)
    {
        await _publisher.Publish(@event, cancellationToken);
    }
}
```

### Job 1: refuse versions that already exist

The repository checks for conflicting changes before saving, but that check and the save are two separate steps. If two requests save the same item at the same moment, both can pass the check before either has stored anything. Without a second check, both would store an event as, say, version 5, and the item's history would no longer make sense.

So the event store must check again, at the moment it stores. The sample does this under a lock: the first new event for an aggregate must be exactly one more than the number of events already stored. A real database does it with a unique key on the aggregate id and version, which makes the second insert fail.

You can see this working in the sample. If six requests check in items against the same version at once, one succeeds and the other five get a `ConcurrencyException`.

### Job 2: publish the events

The read side learns about changes from published events, and it is the event store's job to publish them. The sample's store is given an `IEventPublisher` in its constructor, which is the router, and publishes each event after storing it.

The order matters. Publishing before the events are safely stored would let the read side show changes that might then fail to save.

Publishing straight after storing has a weakness of its own, though: if the process stops between the two steps, the events are stored but never published, and the read side misses them. The sample accepts that, as it keeps everything in memory anyway. [Taking it to production](7-production.md) describes the usual fix.

## Wiring it up

The repository, the event store and the session are all registered in the web app's `Program.cs`, covered in [part 5](5-web-app.md). The sample wraps the repository in a `CacheRepository`, which keeps recently used aggregates in memory so they don't have to be replayed from the start every time. The [caching guide](../guides/caching.md) explains how that works.

Next: [the read model](4-read-model.md).

# Caching

Read this when the same aggregates are loaded over and over, and replaying their events shows up in your timings. `CacheRepository` keeps aggregates in memory between commands, so loading one usually means reading only the events stored since it was cached.

## Wiring it up

`CacheRepository` wraps the normal repository. The cache itself must be a singleton, so all requests share it:

```csharp
builder.Services.AddSingleton<ICache, MemoryCache>();
builder.Services.AddScoped<IRepository>(sp =>
{
    var eventStore = sp.GetRequiredService<IEventStore>();
    return new CacheRepository(new Repository(eventStore), eventStore, sp.GetRequiredService<ICache>());
});
```

`MemoryCache` is CQRSlite's `ICache` on top of `Microsoft.Extensions.Caching.Memory`. It has its own `IMemoryCache` instance, separate from any you register yourself. An aggregate stays cached until it hasn't been used for 15 minutes.

## How it behaves

The cache *lends* an aggregate to one caller at a time:

- **`Get`** takes the aggregate out of the cache. Then it asks the event store for any events stored after the cached version, and applies them, so the aggregate is up to date even if another process changed it. If the versions don't line up, it throws the cached copy away and loads the aggregate from scratch. If the aggregate isn't cached, it is loaded through the inner repository.
- **`Save`** saves through the inner repository and, if that succeeds, puts the aggregate back in the cache for the next caller.
- **A failed `Save`** removes the aggregate from the cache.
- **An aggregate that is never saved**, for example because its command threw, never goes back. The next `Get` loads it from the event store.

Callers never share an aggregate instance. That matters because aggregates are ordinary mutable objects. If two requests got the same instance, each could see the other's uncommitted changes, and one request's commit would save both requests' changes.

With the session this works out naturally: a command handler gets an aggregate, changes it and commits, which saves it and returns it to the cache. `Session.Commit` saves every aggregate the session loaded, changed or not, so a handler that only reads an aggregate also returns it, as long as it commits.

## Concurrency

`CacheRepository` makes `Get` and `Save` for the same aggregate run one at a time within a process, using a lock per aggregate id.

That lock only covers one process. If you run several instances of your application, each has its own cache, and they can hold different versions of the same aggregate. That's still safe. Each `Get` catches up with the event store, and when two instances save the same version, the [event store's version check](event-store.md#rejecting-concurrent-writes) refuses the second one with a `ConcurrencyException`. Correctness always comes from the event store. The cache only saves work.

## With snapshots

To use snapshots as well, put `CacheRepository` outermost, around `SnapshotRepository`, around `Repository`:

```csharp
builder.Services.AddSingleton<ICache, MemoryCache>();
builder.Services.AddSingleton<ISnapshotStore, InMemorySnapshotStore>();
builder.Services.AddSingleton<ISnapshotStrategy, DefaultSnapshotStrategy>();
builder.Services.AddScoped<IRepository>(sp =>
{
    var eventStore = sp.GetRequiredService<IEventStore>();
    var snapshots = new SnapshotRepository(
        sp.GetRequiredService<ISnapshotStore>(),
        sp.GetRequiredService<ISnapshotStrategy>(),
        new Repository(eventStore),
        eventStore);
    return new CacheRepository(snapshots, eventStore, sp.GetRequiredService<ICache>());
});
```

A cached aggregate is then returned without touching the snapshot store at all, and a cache miss loads from the latest snapshot. The other way round, `SnapshotRepository` would restore aggregates itself and bypass the cache. See [Snapshots](snapshots.md) for setting them up.

## Memory

The cache has no size limit. Every aggregate used in the last 15 minutes stays in memory. That's fine for most applications. If you have many large aggregates, write your own `ICache` with a size limit, or don't cache.

## Writing your own cache

`ICache` is small:

<!-- snippet: Framework/CQRSlite/Caching/ICache.cs -->
```csharp
public interface ICache
{
    // ...
    Task<bool> IsTracked(Guid id);
    // ...
    Task Set(Guid id, AggregateRoot aggregate);
    // ...
    Task<AggregateRoot?> Get(Guid id);
    // ...
    Task Remove(Guid id);
    // ...
    [Obsolete("CacheRepository no longer uses eviction callbacks. Implementations can leave this empty.")]
    void RegisterEvictionCallback(Action<Guid> action);
}
```

`CacheRepository` only uses `IsTracked`, `Get`, `Set` and `Remove`. `Get` may return `null` if the entry disappeared since `IsTracked` was called. `RegisterEvictionCallback` is obsolete: `CacheRepository` no longer calls it, so an implementation can leave it empty.

A distributed cache such as Redis is rarely worth it here. A cached aggregate is a live object with private state, so you would have to serialize it, which aggregates aren't designed for. The lending above would also no longer hold across processes. Every `Get` already asks the event store for newer events, so a shared cache saves little over loading from a snapshot.

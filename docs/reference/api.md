# CQRSlite API Reference

Every public type and member in CQRSlite. For how to use them, start with the [tutorial](../tutorial/1-overview.md), and see the [guides](../README.md#guides) for each topic in depth.

## Table of Contents
- [Core Interfaces](#core-interfaces)
- [Handler Interfaces](#handler-interfaces)
- [Message Routing Interfaces](#message-routing-interfaces)
- [Base Classes](#base-classes)
- [Domain and Repository](#domain-and-repository)
- [Event Store](#event-store)
- [Snapshotting](#snapshotting)
- [Caching](#caching)
- [Routing](#routing)
- [Exceptions](#exceptions)

## Core Interfaces

### IMessage

**Namespace:** `CQRSlite.Messages`

Marker interface for all messages (commands, events, queries).

```csharp
public interface IMessage { }
```

**Usage:**
All commands, events, and queries must implement this interface (through their specific interfaces).

---

### ICommand

**Namespace:** `CQRSlite.Commands`

Marker interface for commands. Commands represent intentions to change state.

```csharp
public interface ICommand : IMessage { }
```

**Example:**
```csharp
public class CreateInventoryItem : ICommand
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
```

**Best Practices:**
- Use imperative naming: `CreateProduct`, `UpdatePrice`, `DeleteOrder`
- Include all necessary data for the command
- Consider including `ExpectedVersion` for optimistic concurrency

---

### IEvent

**Namespace:** `CQRSlite.Events`

Interface for domain events. Events represent facts that have occurred.

```csharp
public interface IEvent : IMessage
{
    Guid Id { get; set; }
    int Version { get; set; }
    DateTimeOffset TimeStamp { get; set; }
}
```

**Properties:**
- `Id`: The aggregate identifier (set to the aggregate's Id on save if left empty)
- `Version`: The version of the aggregate after this event (set by the framework on save)
- `TimeStamp`: When the event was saved (UTC, set by the framework)

**Example:**
```csharp
public class InventoryItemCreated : IEvent
{
    public Guid Id { get; set; }
    public int Version { get; set; }
    public DateTimeOffset TimeStamp { get; set; }

    public string Name { get; set; }
    public int InitialCount { get; set; }
}
```

**Best Practices:**
- Use past tense naming: `ProductCreated`, `PriceUpdated`, `OrderDeleted`
- Events are immutable - never change after creation
- Include all necessary data (events should be self-contained)
- Consider versioning strategy for schema evolution

---

### IQuery<TReturn>

**Namespace:** `CQRSlite.Queries`

Interface for queries. Queries represent requests for information.

```csharp
public interface IQuery<TReturn> : IMessage { }
```

**Generic Parameters:**
- `TReturn`: The type of data this query returns

**Example:**
```csharp
public class GetInventoryItemDetails : IQuery<InventoryItemDetailsDto>
{
    public Guid Id { get; set; }
}

public class InventoryItemDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int CurrentCount { get; set; }
    public int Version { get; set; }
}
```

**Best Practices:**
- Queries should not modify state
- Name clearly indicates what is being queried
- Return DTOs, not domain entities

---

## Handler Interfaces

### IHandler<T> / ICancellableHandler<T>

**Namespace:** `CQRSlite.Messages`

Base interfaces for command and event handlers. `RouteRegistrar` discovers handlers through these (and the query handler interfaces).

```csharp
public interface IHandler<in T> where T : IMessage
{
    Task Handle(T message);
}

public interface ICancellableHandler<in T> where T : IMessage
{
    Task Handle(T message, CancellationToken token = default);
}
```

---

### ICommandHandler<T>

**Namespace:** `CQRSlite.Commands`

Interface for handling commands.

```csharp
public interface ICommandHandler<in T> : IHandler<T> where T : ICommand
{
    // Inherits Task Handle(T message)
}
```

**Example:**
```csharp
public class InventoryCommandHandlers : ICommandHandler<CreateInventoryItem>
{
    private readonly ISession _session;

    public InventoryCommandHandlers(ISession session)
    {
        _session = session;
    }

    public async Task Handle(CreateInventoryItem message)
    {
        var item = new InventoryItem(message.Id, message.Name);
        await _session.Add(item);
        await _session.Commit();
    }
}
```

**Rules:**
- Exactly one handler per command type
- Sending throws `InvalidOperationException` if no handler or more than one handler is registered

---

### ICancellableCommandHandler<T>

**Namespace:** `CQRSlite.Commands`

Interface for handling commands with cancellation support.

```csharp
public interface ICancellableCommandHandler<in T> : ICancellableHandler<T> where T : ICommand
{
    // Inherits Task Handle(T message, CancellationToken token = default)
}
```

**Example:**
```csharp
public class InventoryCommandHandlers : ICancellableCommandHandler<CreateInventoryItem>
{
    private readonly ISession _session;

    public InventoryCommandHandlers(ISession session)
    {
        _session = session;
    }

    public async Task Handle(CreateInventoryItem message, CancellationToken token)
    {
        var item = new InventoryItem(message.Id, message.Name);
        await _session.Add(item, token);
        await _session.Commit(token);
    }
}
```

---

### IEventHandler<T>

**Namespace:** `CQRSlite.Events`

Interface for handling events.

```csharp
public interface IEventHandler<in T> : IHandler<T> where T : IEvent
{
    // Inherits Task Handle(T message)
}
```

**Example:**
```csharp
public class InventoryItemDetailView : IEventHandler<InventoryItemCreated>
{
    private readonly IReadDatabase _database;

    public async Task Handle(InventoryItemCreated message)
    {
        await _database.Insert(new InventoryItemDetailsDto
        {
            Id = message.Id,
            Name = message.Name,
            CurrentCount = 0,
            Version = message.Version
        });
    }
}
```

**Rules:**
- Zero or more handlers per event type; publishing with no handlers does nothing
- Handlers are started one after another and awaited together via `Task.WhenAll`, so async handlers run concurrently
- A handler that throws synchronously stops later handlers from starting; `WhenAll` rethrows handler exceptions

---

### ICancellableEventHandler<T>

**Namespace:** `CQRSlite.Events`

Interface for handling events with cancellation support.

```csharp
public interface ICancellableEventHandler<in T> : ICancellableHandler<T> where T : IEvent
{
    // Inherits Task Handle(T message, CancellationToken token = default)
}
```

---

### IQueryHandler<T, TResponse>

**Namespace:** `CQRSlite.Queries`

Interface for handling queries.

```csharp
public interface IQueryHandler<in T, TResponse> where T : IQuery<TResponse>
{
    Task<TResponse> Handle(T query);
}
```

**Generic Parameters:**
- `T`: The query type
- `TResponse`: The return type

**Example:**
```csharp
public class InventoryItemDetailView :
    IQueryHandler<GetInventoryItemDetails, InventoryItemDetailsDto>
{
    private readonly IReadDatabase _database;

    public async Task<InventoryItemDetailsDto> Handle(GetInventoryItemDetails message)
    {
        return await _database.GetById(message.Id);
    }
}
```

**Rules:**
- Exactly one handler per query type
- Must return `TResponse`

---

### ICancellableQueryHandler<T, TResponse>

**Namespace:** `CQRSlite.Queries`

Interface for handling queries with cancellation support.

```csharp
public interface ICancellableQueryHandler<in T, TResponse> where T : IQuery<TResponse>
{
    Task<TResponse> Handle(T message, CancellationToken token = default);
}
```

---

## Message Routing Interfaces

### ICommandSender

**Namespace:** `CQRSlite.Commands`

Interface for sending commands to handlers.

```csharp
public interface ICommandSender
{
    Task Send<T>(T command, CancellationToken cancellationToken = default) where T : class, ICommand;
}
```

**Usage:**
```csharp
public class ProductController
{
    private readonly ICommandSender _commandSender;

    [HttpPost]
    public async Task<IActionResult> Create(CreateProduct command, CancellationToken token)
    {
        await _commandSender.Send(command, token);
        return Ok();
    }
}
```

**Behavior:**
- Routes command to the handler registered for its exact runtime type
- Throws `InvalidOperationException` if no handler or multiple handlers registered
- Executes handler asynchronously

---

### IEventPublisher

**Namespace:** `CQRSlite.Events`

Interface for publishing events to handlers.

```csharp
public interface IEventPublisher
{
    Task Publish<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent;
}
```

Usually called by your event store, once it has stored the events. See [Publishing events](../guides/event-store.md#publishing-events).

**Behavior:**
- Routes event to all handlers registered for its runtime type; does nothing if there are none
- Starts handlers one after another, so a synchronous throw stops later handlers from starting
- Returns when all handlers complete; `Task.WhenAll` rethrows handler exceptions

---

### IQueryProcessor

**Namespace:** `CQRSlite.Queries`

Interface for processing queries.

```csharp
public interface IQueryProcessor
{
    Task<TResponse> Query<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
```

**Usage:**
```csharp
public class ProductController
{
    private readonly IQueryProcessor _queryProcessor;

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> Get(Guid id)
    {
        var result = await _queryProcessor.Query(new GetProduct { Id = id });
        return Ok(result);
    }
}
```

**Behavior:**
- Routes query to registered handler
- Throws if no handler or multiple handlers registered
- Returns typed result from handler

---

### IHandlerRegistrar

**Namespace:** `CQRSlite.Routing`

Interface for registering message handlers.

```csharp
public interface IHandlerRegistrar
{
    void RegisterHandler<T>(Func<T, CancellationToken, Task> handler) where T : class, IMessage;
}
```

**Usage (Manual Registration):**
```csharp
var registrar = app.Services.GetRequiredService<IHandlerRegistrar>();
// Resolve handlers from the current request scope (RequestServiceProvider as in Sample/CQRSWeb/Program.cs)
var services = new RequestServiceProvider(app.Services);

registrar.RegisterHandler<CreateProduct>((cmd, token) =>
    services.GetRequiredService<ProductCommandHandler>().Handle(cmd));
```

**Usage (Automatic Registration):**
```csharp
new RouteRegistrar(new RequestServiceProvider(app.Services))
    .RegisterInAssemblyOf(typeof(ProductCommandHandler));
```

See [Handlers and routing](../guides/handlers-and-routing.md).

---

## Base Classes

### AggregateRoot

**Namespace:** `CQRSlite.Domain`

Base class for all aggregates using event sourcing. See the [aggregates guide](../guides/aggregates.md) for the rules aggregates must follow.

```csharp
public abstract class AggregateRoot
{
    public Guid Id { get; protected set; }
    public int Version { get; protected set; }

    protected void ApplyChange(IEvent @event);
    protected virtual void ApplyEvent(IEvent @event);

    // Used by the repositories, public so custom repositories and tests can use them
    public IEvent[] GetUncommittedChanges();
    public IEvent[] FlushUncommittedChanges();
    public void LoadFromHistory(IEnumerable<IEvent> history);
}
```

**Key Methods:**

#### ApplyChange(IEvent @event)

Applies a new event to the aggregate and records it as uncommitted.

**Usage:**
```csharp
public class Product : AggregateRoot
{
    public void ChangePrice(decimal newPrice)
    {
        // Validate business rules
        if (newPrice < 0)
            throw new ArgumentException("Price cannot be negative");

        // Apply and record event
        ApplyChange(new ProductPriceChanged { Id = Id, NewPrice = newPrice });
    }
}
```

**Behavior:**
1. Calls `ApplyEvent()` to update internal state
2. Adds event to uncommitted changes list

`Version` is not changed here. When the aggregate is saved, `FlushUncommittedChanges()` sets each event's `Id` (if empty), `Version` and `TimeStamp`, and increases the aggregate's `Version`.

#### ApplyEvent(IEvent @event)

Virtual method that applies an event to aggregate state. Uses convention-based routing by default.

**Default Behavior:**
Searches for a method (any visibility) with signature: `Apply(EventType @event)`. If none is found, the event is ignored.

**Example:**
```csharp
public class Product : AggregateRoot
{
    private string _name;
    private decimal _price;

    // Convention-based event application
    private void Apply(ProductCreated e)
    {
        _name = e.Name;
        _price = e.Price;
    }

    private void Apply(ProductPriceChanged e)
    {
        _price = e.NewPrice;
    }
}
```

**Custom Override:**
```csharp
protected override void ApplyEvent(IEvent @event)
{
    switch (@event)
    {
        case ProductCreated e:
            _name = e.Name;
            _price = e.Price;
            break;
        case ProductPriceChanged e:
            _price = e.NewPrice;
            break;
        default:
            base.ApplyEvent(@event); // Fall back to convention
            break;
    }
}
```

**Constructor Requirements:**
- Public/protected constructor for creating new aggregates
- Parameterless constructor (public or private) for rehydration

```csharp
public class Product : AggregateRoot
{
    // Constructor for new aggregates
    public Product(Guid id, string name, decimal price)
    {
        Id = id;
        ApplyChange(new ProductCreated { Id = id, Name = name, Price = price });
    }

    // Required for rehydration
    private Product() { }
}
```

---

### SnapshotAggregateRoot<T>

**Namespace:** `CQRSlite.Snapshotting`

Base class for aggregates that support snapshotting.

```csharp
public abstract class SnapshotAggregateRoot<T> : AggregateRoot
    where T : Snapshot
{
    protected abstract T CreateSnapshot();
    protected abstract void RestoreFromSnapshot(T snapshot);

    // Used by SnapshotRepository
    public T GetSnapshot();          // CreateSnapshot() with Id set
    public void Restore(T snapshot); // Sets Id and Version, then RestoreFromSnapshot()
}
```

**Key Methods:**

#### CreateSnapshot()

Creates a snapshot of the current aggregate state.

**Example:**
```csharp
public class Product : SnapshotAggregateRoot<ProductSnapshot>
{
    private string _name;
    private decimal _price;
    private bool _discontinued;

    protected override ProductSnapshot CreateSnapshot()
    {
        // Id and Version are set by the framework
        return new ProductSnapshot
        {
            Name = _name,
            Price = _price,
            Discontinued = _discontinued
        };
    }
}
```

#### RestoreFromSnapshot(T snapshot)

Restores aggregate state from a snapshot.

**Example:**
```csharp
protected override void RestoreFromSnapshot(ProductSnapshot snapshot)
{
    _name = snapshot.Name;
    _price = snapshot.Price;
    _discontinued = snapshot.Discontinued;
}
```

---

### Snapshot

**Namespace:** `CQRSlite.Snapshotting`

Base class for snapshot objects.

```csharp
public abstract class Snapshot
{
    public Guid Id { get; set; }
    public int Version { get; set; }
}
```

**Example:**
```csharp
public class ProductSnapshot : Snapshot
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool Discontinued { get; set; }
}
```

**Best Practices:**
- Include all state necessary to rebuild the aggregate
- Keep snapshots serializable (for storage)
- Consider versioning for snapshot evolution

---

## Domain and Repository

### IRepository

**Namespace:** `CQRSlite.Domain`

Interface for aggregate persistence.

```csharp
public interface IRepository
{
    Task Save<T>(T aggregate, int? expectedVersion = null, CancellationToken cancellationToken = default)
        where T : AggregateRoot;

    Task<T> Get<T>(Guid aggregateId, CancellationToken cancellationToken = default)
        where T : AggregateRoot;
}
```

**Methods:**

#### Save<T>(T aggregate, int? expectedVersion, CancellationToken)

Saves an aggregate by persisting its uncommitted events.

**Parameters:**
- `aggregate`: The aggregate to save
- `expectedVersion`: Expected version for optimistic concurrency (optional). If `null`, there is no check: events saved since the aggregate was loaded are applied to it first, then the new events are appended
- `cancellationToken`: Cancellation token

**Throws:**
- `ConcurrencyException`: If the event store has events after `expectedVersion`. This check is not atomic, so the event store must also reject duplicate (Id, Version) events (see [IEventStore](#ieventstore))

**Example:**
```csharp
public async Task Handle(ChangeProductPrice command)
{
    var product = await _repository.Get<Product>(command.Id);
    product.ChangePrice(command.NewPrice);
    await _repository.Save(product, command.ExpectedVersion);
}
```

#### Get<T>(Guid aggregateId, CancellationToken)

Loads an aggregate by replaying its event history.

**Parameters:**
- `aggregateId`: The aggregate identifier
- `cancellationToken`: Cancellation token

**Returns:**
The rehydrated aggregate

**Throws:**
- `AggregateNotFoundException`: If no events found for aggregate

**Example:**
```csharp
var product = await _repository.Get<Product>(productId);
```

---

### Repository

**Namespace:** `CQRSlite.Domain`

Default implementation of `IRepository`.

```csharp
public class Repository : IRepository
{
    public Repository(IEventStore eventStore);

    [Obsolete("The eventstore should publish events after saving")]
    public Repository(IEventStore eventStore, IEventPublisher publisher);
}
```

**Constructor Parameters:**
- `eventStore`: Event store for loading/saving events
- `publisher`: Event publisher that gets every saved event (obsolete - publish from the event store instead; must not be null)

**Usage:**
```csharp
builder.Services.AddScoped<IRepository>(sp =>
    new Repository(sp.GetRequiredService<IEventStore>()));
```

---

### ISession

**Namespace:** `CQRSlite.Domain`

Interface for Unit of Work pattern implementation.

```csharp
public interface ISession
{
    Task Add<T>(T aggregate, CancellationToken cancellationToken = default) where T : AggregateRoot;
    Task<T> Get<T>(Guid id, int? expectedVersion = null, CancellationToken cancellationToken = default) where T : AggregateRoot;
    Task Commit(CancellationToken cancellationToken = default);
}
```

**Methods:**

#### Add<T>(T aggregate, CancellationToken)

Adds a new aggregate to be tracked by the session. Throws `ConcurrencyException` if a different instance with the same Id is already tracked.

**Example:**
```csharp
var product = new Product(Guid.NewGuid(), "New Product", 99.99m);
await _session.Add(product);
await _session.Commit();
```

#### Get<T>(Guid id, int? expectedVersion, CancellationToken)

Gets an aggregate, either from the session tracking or from the repository.

**Parameters:**
- `id`: Aggregate identifier
- `expectedVersion`: Expected version for optimistic concurrency
- `cancellationToken`: Cancellation token

**Returns:**
The aggregate

**Throws:**
- `ConcurrencyException`: If expectedVersion doesn't match the aggregate's version
- `AggregateNotFoundException`: If the repository finds no events for the aggregate

**Example:**
```csharp
var product = await _session.Get<Product>(productId, expectedVersion: 5);
product.ChangePrice(150m);
await _session.Commit();
```

#### Commit(CancellationToken)

Stops tracking all aggregates, then saves each of them to the repository in turn. This is not atomic: if one save fails, aggregates saved before it stay saved.

**Example:**
```csharp
var product = await _session.Get<Product>(productId);
product.ChangePrice(150m);

var category = await _session.Get<Category>(categoryId);
category.AddProduct(productId);

// Saves both aggregates, one after the other
await _session.Commit();
```

---

### Session

**Namespace:** `CQRSlite.Domain`

Default implementation of `ISession`.

```csharp
public class Session : ISession
{
    public Session(IRepository repository)
}
```

**Usage:**
```csharp
builder.Services.AddScoped<ISession, Session>();
```

**Behavior:**
- Unit of work (not a transaction): tracks aggregates in memory until `Commit`
- Prevents duplicate loads of same aggregate
- Saves all tracked aggregates on commit, one at a time
- Passes the version each aggregate had when it was added or loaded as `expectedVersion` (0 for a new aggregate)

---

## Event Store

### IEventStore

**Namespace:** `CQRSlite.Events`

Interface for event persistence. **You must implement this interface.**

```csharp
public interface IEventStore
{
    Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default);
    Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default);
}
```

**Methods:**

#### Save(IEnumerable<IEvent> events, CancellationToken)

Persists events and publishes them.

**Concurrency:** `Repository`'s `ConcurrencyException` check (reading events after `expectedVersion` before saving) is check-then-act, not atomic. Your event store's `Save` must itself reject an event whose (aggregate Id, Version) already exists, e.g. with a unique index or primary key on `(AggregateId, Version)`. Otherwise two concurrent writers can both succeed.

Store the events (which already have `Id`, `Version` and `TimeStamp` set) all or none, then publish them. [Implementing an event store](../guides/event-store.md) has a complete SQL example.

#### Get(Guid aggregateId, int fromVersion, CancellationToken)

Retrieves events for an aggregate starting from a specific version.

**Parameters:**
- `aggregateId`: The aggregate identifier
- `fromVersion`: Starting version (exclusive - events after this version). `-1` means from the start; `Repository.Get` passes `-1`
- `cancellationToken`: Cancellation token

**Returns:**
Events in order by version

Return an empty list, not `null`, when there are none.

---

## Snapshotting

### ISnapshotStore

**Namespace:** `CQRSlite.Snapshotting`

Interface for snapshot persistence.

```csharp
public interface ISnapshotStore
{
    Task<Snapshot?> Get(Guid id, CancellationToken cancellationToken = default);
    Task Save(Snapshot snapshot, CancellationToken cancellationToken = default);
}
```

**Methods:**

#### Get(Guid id, CancellationToken)

Retrieves the latest snapshot for an aggregate.

**Returns:**
- The latest snapshot, or `null` if none exists

#### Save(Snapshot snapshot, CancellationToken)

Saves a snapshot.

Keep only the latest snapshot per aggregate. See [Snapshots](../guides/snapshots.md#storing-snapshots).

---

### ISnapshotStrategy

**Namespace:** `CQRSlite.Snapshotting`

Interface for snapshot strategies.

```csharp
public interface ISnapshotStrategy
{
    bool IsSnapshotable(Type aggregateType);
    bool ShouldMakeSnapShot(AggregateRoot aggregate);
}
```

**Methods:**

#### IsSnapshotable(Type aggregateType)

Determines if an aggregate type supports snapshots.

#### ShouldMakeSnapShot(AggregateRoot aggregate)

Determines if a snapshot should be created for the aggregate.

---

### DefaultSnapshotStrategy

**Namespace:** `CQRSlite.Snapshotting`

Default implementation that snapshots every 100 events.

```csharp
public class DefaultSnapshotStrategy : ISnapshotStrategy
{
    public DefaultSnapshotStrategy();                // Every 100 events
    public DefaultSnapshotStrategy(ushort interval); // Every `interval` events; throws ArgumentOutOfRangeException if 0

    public bool IsSnapshotable(Type aggregateType);
    public bool ShouldMakeSnapShot(AggregateRoot aggregate);
}
```

**Behavior:**
- `IsSnapshotable`: true if the type derives from `SnapshotAggregateRoot<>`
- `ShouldMakeSnapShot`: true if the version plus the uncommitted changes crosses a multiple of the interval. It is called before saving, while the new events are still uncommitted

For a custom strategy, see [Snapshots](../guides/snapshots.md#when-snapshots-are-taken).

---

### SnapshotRepository

**Namespace:** `CQRSlite.Snapshotting`

Repository decorator that adds snapshot support.

```csharp
public class SnapshotRepository : IRepository
{
    public SnapshotRepository(
        ISnapshotStore snapshotStore,
        ISnapshotStrategy snapshotStrategy,
        IRepository repository,
        IEventStore eventStore)
}
```

**Usage:**
```csharp
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

**Behavior:**
- **Get**: If the type is snapshotable and a snapshot exists, restores it and applies the events after the snapshot version; otherwise loads through the inner repository
- **Save**: Decides before saving whether a snapshot is due (see `ISnapshotStrategy.ShouldMakeSnapShot`), saves the events through the inner repository, and only after that succeeds saves a snapshot of the aggregate's state after the save (with the cancellation token)

---

## Caching

### ICache

**Namespace:** `CQRSlite.Caching`

Interface for cache implementations.

```csharp
public interface ICache
{
    Task<bool> IsTracked(Guid id);
    Task Set(Guid id, AggregateRoot aggregate);
    Task<AggregateRoot?> Get(Guid id);
    Task Remove(Guid id);
    [Obsolete] void RegisterEvictionCallback(Action<Guid> action);
}
```

---

### MemoryCache

**Namespace:** `CQRSlite.Caching`

Default in-memory cache implementation using `Microsoft.Extensions.Caching.Memory`. Entries have a 15-minute sliding expiration.

```csharp
public class MemoryCache : ICache
{
    public MemoryCache()
}
```

**Usage:**
```csharp
builder.Services.AddSingleton<ICache, MemoryCache>();
```

---

### CacheRepository

**Namespace:** `CQRSlite.Caching`

Thread-safe repository decorator that keeps aggregates in memory between saves. See [Caching](../guides/caching.md).

```csharp
public class CacheRepository : IRepository
{
    public CacheRepository(IRepository repository, IEventStore eventStore, ICache cache)
}
```

**Usage:**
```csharp
builder.Services.AddSingleton<ICache, MemoryCache>();
builder.Services.AddScoped<IRepository>(sp =>
{
    var eventStore = sp.GetRequiredService<IEventStore>();
    return new CacheRepository(new Repository(eventStore), eventStore, sp.GetRequiredService<ICache>());
});
```

**Behavior:**
- Lends a cached aggregate to one caller at a time, so callers never share an instance
- **Get**: Takes the aggregate out of the cache and applies any newer events from the event store. If the versions don't line up, or it isn't cached, loads it through the inner repository
- **Save**: Saves through the inner repository, then puts the aggregate back in the cache. If saving throws, removes it from the cache
- An aggregate that is never saved is not put back; the next `Get` loads it again
- `Get` and `Save` for the same aggregate id run one at a time within the process

---

## Routing

### Router

**Namespace:** `CQRSlite.Routing`

Central message router implementing all routing interfaces.

```csharp
public class Router : IHandlerRegistrar, ICommandSender, IEventPublisher, IQueryProcessor
```

**Usage:**
```csharp
var router = new Router();
builder.Services.AddSingleton(router);
builder.Services.AddSingleton<ICommandSender>(router);
builder.Services.AddSingleton<IEventPublisher>(router);
builder.Services.AddSingleton<IQueryProcessor>(router);
builder.Services.AddSingleton<IHandlerRegistrar>(router);
```

---

### RouteRegistrar

**Namespace:** `CQRSlite.Routing`

Automatic handler registration via reflection.

```csharp
public class RouteRegistrar
{
    public RouteRegistrar(IServiceProvider serviceLocator);

    // Scan the assemblies of the given types for handlers
    public void RegisterInAssemblyOf(params Type[] typesFromAssemblyContainingMessages);

    // Register the given handler types
    public void RegisterHandlers(params Type[] handlers);
}
```

`IHandlerRegistrar` must be resolvable from the service provider. Each handler is resolved from it every time a message is routed, so handlers must be registered in DI, and in ASP.NET Core the provider should resolve from the current request scope (see `RequestServiceProvider` in Sample/CQRSWeb/Program.cs) so scoped services like `ISession` aren't resolved from the root provider.

**Usage:**
```csharp
var registrar = new RouteRegistrar(new RequestServiceProvider(app.Services));

// Register all handlers in the assembly containing ProductCommandHandler
registrar.RegisterInAssemblyOf(typeof(ProductCommandHandler));

// Or register specific handler types
registrar.RegisterHandlers(typeof(ProductCommandHandler), typeof(ProductEventHandler));
```

---

## Exceptions

### ConcurrencyException

**Namespace:** `CQRSlite.Domain.Exception`

Thrown when optimistic concurrency check fails.

```csharp
public class ConcurrencyException : Exception
{
    public ConcurrencyException(Guid id); // Message names the aggregate id
}
```

**Example:**
```csharp
try
{
    await _repository.Save(product, expectedVersion: 5);
}
catch (ConcurrencyException ex)
{
    // Handle conflict - retry, merge, or inform user
    Console.WriteLine($"Concurrency conflict: {ex.Message}");
}
```

---

### AggregateNotFoundException

**Namespace:** `CQRSlite.Domain.Exception`

Thrown when aggregate cannot be found.

```csharp
public class AggregateNotFoundException : Exception
{
    public AggregateNotFoundException(Type t, Guid id); // Message names the type and id
}
```

**Example:**
```csharp
try
{
    var product = await _repository.Get<Product>(productId);
}
catch (AggregateNotFoundException ex)
{
    return NotFound(ex.Message);
}
```

---

### AggregateOrEventMissingIdException

**Namespace:** `CQRSlite.Domain.Exception`

Thrown when aggregate or event is missing required Id.

---

### MissingParameterLessConstructorException

**Namespace:** `CQRSlite.Domain.Exception`

Thrown when aggregate lacks required parameterless constructor for rehydration.

```csharp
public class MissingParameterLessConstructorException : Exception
{
    public MissingParameterLessConstructorException(Type type);
}
```

**Resolution:**
Add private parameterless constructor:
```csharp
public class Product : AggregateRoot
{
    private Product() { } // Required for rehydration
}
```

---

### EventIdIncorrectException

**Namespace:** `CQRSlite.Domain.Exception`

Thrown when an event's Id differs from its aggregate's Id, on save or when loading history.

---

### EventsOutOfOrderException

**Namespace:** `CQRSlite.Domain.Exception`

Thrown when the event store returns events whose versions don't follow on from the aggregate's version.

---

### HandlerNotResolvedException / ResolvedHandlerMethodNotFoundException

**Namespace:** `CQRSlite.Routing.Exception`

Thrown by handlers registered with `RouteRegistrar` when the service provider returns null for `IHandlerRegistrar` or a handler type, or when the handler's `Handle` method can't be invoked. Both derive from `ArgumentNullException`.

---

This API reference covers all public interfaces and classes in CQRSlite. For how to put them together, see the [tutorial](../tutorial/1-overview.md) and the [guides](../README.md#guides).

# CQRSlite API Reference

## Table of Contents
- [Core Interfaces](#core-interfaces)
- [Base Classes](#base-classes)
- [Routing and Handling](#routing-and-handling)
- [Domain and Repository](#domain-and-repository)
- [Snapshotting](#snapshotting)
- [Caching](#caching)
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
- `Id`: The aggregate identifier
- `Version`: The version of the aggregate when this event occurred
- `TimeStamp`: When the event occurred (UTC)

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

### ICommandHandler<T>

**Namespace:** `CQRSlite.Commands`

Interface for handling commands.

```csharp
public interface ICommandHandler<in T> : IHandler<T> where T : ICommand
{
    Task Handle(T message);
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
- Throws `InvalidOperationException` if multiple handlers registered

---

### ICancellableCommandHandler<T>

**Namespace:** `CQRSlite.Commands`

Interface for handling commands with cancellation support.

```csharp
public interface ICancellableCommandHandler<in T> : ICancellableHandler<T> where T : ICommand
{
    Task Handle(T message, CancellationToken token);
}
```

**Example:**
```csharp
public class InventoryCommandHandlers : ICancellableCommandHandler<CreateInventoryItem>
{
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
    Task Handle(T message);
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
- Zero or more handlers per event type
- All handlers execute in parallel via `Task.WhenAll`
- Handler failures don't prevent other handlers from executing

---

### ICancellableEventHandler<T>

**Namespace:** `CQRSlite.Events`

Interface for handling events with cancellation support.

```csharp
public interface ICancellableEventHandler<in T> : ICancellableHandler<T> where T : IEvent
{
    Task Handle(T message, CancellationToken token);
}
```

---

### IQueryHandler<T, TResponse>

**Namespace:** `CQRSlite.Queries`

Interface for handling queries.

```csharp
public interface IQueryHandler<in T, TResponse> where T : IQuery<TResponse>
{
    Task<TResponse> Handle(T message);
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
    Task<TResponse> Handle(T message, CancellationToken token);
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
    Task Send<T>(T command, CancellationToken cancellationToken = default) where T : ICommand;
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
- Routes command to registered handler
- Throws if no handler or multiple handlers registered
- Executes handler asynchronously

---

### IEventPublisher

**Namespace:** `CQRSlite.Events`

Interface for publishing events to handlers.

```csharp
public interface IEventPublisher
{
    Task Publish<T>(T @event, CancellationToken cancellationToken = default) where T : IEvent;
}
```

**Usage:**
```csharp
public class InMemoryEventStore : IEventStore
{
    private readonly IEventPublisher _publisher;

    public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken)
    {
        foreach (var @event in events)
        {
            // Save to storage
            _storage.Add(@event);

            // Publish after save
            await _publisher.Publish(@event, cancellationToken);
        }
    }
}
```

**Behavior:**
- Routes event to all registered handlers
- Handlers execute in parallel
- Returns when all handlers complete

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
    void RegisterHandler<T>(Func<T, CancellationToken, Task> handler) where T : IMessage;
}
```

**Usage (Manual Registration):**
```csharp
var registrar = serviceProvider.GetService<IHandlerRegistrar>();

registrar.RegisterHandler<CreateProduct>(async (cmd, token) =>
{
    var handler = serviceProvider.GetService<ProductCommandHandler>();
    await handler.Handle(cmd, token);
});
```

**Usage (Automatic Registration):**
```csharp
var routeRegistrar = new RouteRegistrar(serviceProvider);
routeRegistrar.Register(typeof(ProductCommandHandler).Assembly);
```

---

## Base Classes

### AggregateRoot

**Namespace:** `CQRSlite.Domain`

Base class for all aggregates using event sourcing.

```csharp
public abstract class AggregateRoot
{
    public Guid Id { get; protected set; }
    public int Version { get; protected set; }

    protected void ApplyChange(IEvent @event);
    protected virtual void ApplyEvent(IEvent @event);

    // Internal methods (used by Repository)
    internal IEnumerable<IEvent> FlushUncommittedChanges();
    internal void LoadFromHistory(IEnumerable<IEvent> history);
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
        ApplyChange(new ProductPriceChanged(Id, newPrice));
    }
}
```

**Behavior:**
1. Calls `ApplyEvent()` to update internal state
2. Adds event to uncommitted changes list
3. Increments version

#### ApplyEvent(IEvent @event)

Virtual method that applies an event to aggregate state. Uses convention-based routing by default.

**Default Behavior:**
Searches for method with signature: `Apply(EventType @event)`

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
- Private parameterless constructor for rehydration

```csharp
public class Product : AggregateRoot
{
    // Constructor for new aggregates
    public Product(Guid id, string name, decimal price)
    {
        Id = id;
        ApplyChange(new ProductCreated(id, name, price));
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
public abstract class SnapshotAggregateRoot<TSnapshot> : AggregateRoot
    where TSnapshot : Snapshot
{
    protected abstract TSnapshot CreateSnapshot();
    protected abstract void RestoreFromSnapshot(TSnapshot snapshot);

    // Internal methods (used by SnapshotRepository)
    internal TSnapshot GetSnapshot();
    internal void Restore(TSnapshot snapshot);
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
        return new ProductSnapshot
        {
            Id = Id,
            Version = Version,
            Name = _name,
            Price = _price,
            Discontinued = _discontinued
        };
    }
}
```

#### RestoreFromSnapshot(TSnapshot snapshot)

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
- `expectedVersion`: Expected version for optimistic concurrency (optional)
- `cancellationToken`: Cancellation token

**Throws:**
- `ConcurrencyException`: If expectedVersion doesn't match current version
- `AggregateNotFoundException`: If aggregate not found when expectedVersion specified

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
    public Repository(IEventStore eventStore, IEventPublisher publisher = null)
}
```

**Constructor Parameters:**
- `eventStore`: Event store for loading/saving events
- `publisher`: Optional event publisher (deprecated - use event store to publish)

**Usage:**
```csharp
services.AddScoped<IRepository>(sp =>
    new Repository(sp.GetService<IEventStore>()));
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

Adds a new aggregate to be tracked by the session.

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
- `ConcurrencyException`: If expectedVersion doesn't match

**Example:**
```csharp
var product = await _session.Get<Product>(productId, expectedVersion: 5);
product.ChangePrice(150m);
await _session.Commit();
```

#### Commit(CancellationToken)

Saves all tracked aggregates to the repository.

**Example:**
```csharp
var product = await _session.Get<Product>(productId);
product.ChangePrice(150m);

var category = await _session.Get<Category>(categoryId);
category.AddProduct(productId);

// Saves both aggregates
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
services.AddScoped<ISession, Session>();
```

**Behavior:**
- Tracks aggregates in memory during transaction
- Prevents duplicate loads of same aggregate
- Saves all tracked aggregates on commit
- Maintains original version for concurrency checking

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

**Best Practice Implementation:**
```csharp
public class SqlEventStore : IEventStore
{
    private readonly IEventPublisher _publisher;
    private readonly IDbConnection _connection;

    public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken)
    {
        using var transaction = _connection.BeginTransaction();

        try
        {
            foreach (var @event in events)
            {
                // Save event to database
                await SaveEventToDatabase(@event);

                // Publish after save
                await _publisher.Publish(@event, cancellationToken);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
```

#### Get(Guid aggregateId, int fromVersion, CancellationToken)

Retrieves events for an aggregate starting from a specific version.

**Parameters:**
- `aggregateId`: The aggregate identifier
- `fromVersion`: Starting version (exclusive - events after this version)
- `cancellationToken`: Cancellation token

**Returns:**
Events in order by version

**Example Implementation:**
```csharp
public async Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken)
{
    var eventRecords = await _connection.QueryAsync<EventRecord>(
        "SELECT * FROM Events WHERE AggregateId = @Id AND Version > @Version ORDER BY Version",
        new { Id = aggregateId, Version = fromVersion });

    return eventRecords.Select(DeserializeEvent);
}
```

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

**Example Implementation:**
```csharp
public class SqlSnapshotStore : ISnapshotStore
{
    public async Task Save(Snapshot snapshot, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(snapshot, snapshot.GetType());
        await _connection.ExecuteAsync(
            "INSERT INTO Snapshots (Id, Version, Type, Data) VALUES (@Id, @Version, @Type, @Data) " +
            "ON CONFLICT (Id) DO UPDATE SET Version = @Version, Data = @Data",
            new
            {
                snapshot.Id,
                snapshot.Version,
                Type = snapshot.GetType().AssemblyQualifiedName,
                Data = json
            });
    }

    public async Task<Snapshot?> Get(Guid id, CancellationToken cancellationToken)
    {
        var record = await _connection.QueryFirstOrDefaultAsync<SnapshotRecord>(
            "SELECT * FROM Snapshots WHERE Id = @Id", new { Id = id });

        if (record == null) return null;

        var type = Type.GetType(record.Type);
        return (Snapshot)JsonSerializer.Deserialize(record.Data, type);
    }
}
```

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
    private const int SnapshotInterval = 100;

    public bool IsSnapshotable(Type aggregateType)
    {
        return aggregateType.GetInterfaces().Any(i =>
            i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(ISnapshotAggregate<>));
    }

    public bool ShouldMakeSnapShot(AggregateRoot aggregate)
    {
        if (!IsSnapshotable(aggregate.GetType())) return false;

        var i = aggregate.Version;
        return i >= SnapshotInterval && i % SnapshotInterval == 0;
    }
}
```

**Custom Strategy Example:**
```csharp
public class CustomSnapshotStrategy : ISnapshotStrategy
{
    public bool IsSnapshotable(Type aggregateType)
    {
        return typeof(ISnapshotAggregate<>).IsAssignableFrom(aggregateType);
    }

    public bool ShouldMakeSnapShot(AggregateRoot aggregate)
    {
        // Snapshot after 50 events, then every 100
        if (aggregate.Version == 50) return true;
        return aggregate.Version >= 100 && aggregate.Version % 100 == 0;
    }
}
```

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
services.AddScoped<IRepository>(sp =>
    new SnapshotRepository(
        sp.GetService<ISnapshotStore>(),
        sp.GetService<ISnapshotStrategy>(),
        new Repository(sp.GetService<IEventStore>()),
        sp.GetService<IEventStore>()));
```

**Behavior:**
- **Get**: Tries to load from snapshot, then applies events after snapshot version
- **Save**: Saves aggregate, then checks if snapshot should be created

---

## Caching

### ICache

**Namespace:** `CQRSlite.Caching`

Interface for cache implementations.

```csharp
public interface ICache
{
    bool IsTracked(Guid id);
    void Set(Guid id, AggregateRoot aggregate);
    AggregateRoot Get(Guid id);
    void Remove(Guid id);
    void RegisterEvictionCallback(Action<Guid> action);
}
```

---

### MemoryCache

**Namespace:** `CQRSlite.Caching`

Default in-memory cache implementation using `Microsoft.Extensions.Caching.Memory`.

```csharp
public class MemoryCache : ICache
{
    public MemoryCache()
}
```

**Usage:**
```csharp
services.AddSingleton<ICache, MemoryCache>();
```

---

### CacheRepository

**Namespace:** `CQRSlite.Caching`

Thread-safe repository decorator that adds caching.

```csharp
public class CacheRepository : IRepository
{
    public CacheRepository(IRepository repository, IEventStore eventStore, ICache cache)
}
```

**Usage:**
```csharp
services.AddSingleton<ICache, MemoryCache>();
services.AddScoped<IRepository>(sp =>
    new CacheRepository(
        new Repository(sp.GetService<IEventStore>()),
        sp.GetService<IEventStore>(),
        sp.GetService<ICache>()));
```

**Behavior:**
- **Get**: Checks cache first, applies new events if cached
- **Save**: Invalidates cache
- Thread-safe per-aggregate using semaphores
- Detects and handles skipped events

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
services.AddSingleton(router);
services.AddSingleton<ICommandSender>(router);
services.AddSingleton<IEventPublisher>(router);
services.AddSingleton<IQueryProcessor>(router);
services.AddSingleton<IHandlerRegistrar>(router);
```

---

### RouteRegistrar

**Namespace:** `CQRSlite.Routing`

Automatic handler registration via reflection.

```csharp
public class RouteRegistrar
{
    public RouteRegistrar(IServiceProvider serviceProvider)

    public void Register(Assembly assembly)
    public void Register(params Type[] typesFromAssemblies)
}
```

**Usage:**
```csharp
var registrar = new RouteRegistrar(serviceProvider);

// Register all handlers from assembly
registrar.Register(typeof(ProductCommandHandler).Assembly);

// Register specific types
registrar.Register(typeof(ProductCommandHandler), typeof(ProductEventHandler));
```

---

## Exceptions

### ConcurrencyException

**Namespace:** `CQRSlite.Domain.Exception`

Thrown when optimistic concurrency check fails.

```csharp
public class ConcurrencyException : Exception
{
    public Guid Id { get; }
    public int ExpectedVersion { get; }
    public int ActualVersion { get; }
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
    Console.WriteLine($"Concurrency conflict: Expected v{ex.ExpectedVersion}, was v{ex.ActualVersion}");
}
```

---

### AggregateNotFoundException

**Namespace:** `CQRSlite.Domain.Exception`

Thrown when aggregate cannot be found.

```csharp
public class AggregateNotFoundException : Exception
{
    public Guid Id { get; }
    public Type Type { get; }
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
    return NotFound($"Product {ex.Id} not found");
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
    public Type AggregateType { get; }
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

This API reference covers all public interfaces and classes in CQRSlite. For implementation examples and best practices, see [DEVELOPER.md](./DEVELOPER.md).

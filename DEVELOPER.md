# CQRSlite Developer Documentation

## Table of Contents
- [Architecture Overview](#architecture-overview)
- [Core Concepts](#core-concepts)
- [Getting Started](#getting-started)
- [Implementation Guide](#implementation-guide)
- [Extension Points](#extension-points)
- [Best Practices](#best-practices)
- [Testing](#testing)
- [Performance Considerations](#performance-considerations)
- [Contributing](#contributing)

## Architecture Overview

CQRSlite is a lightweight CQRS (Command Query Responsibility Segregation) and Event Sourcing framework for .NET. It provides the essential building blocks for implementing CQRS/ES patterns while maintaining flexibility and pluggability.

### Design Principles

1. **Minimal Dependencies**: Only depends on `Microsoft.Extensions.Caching.Memory`
2. **Pluggability**: Every component can be replaced with custom implementations
3. **Convention over Configuration**: Uses convention-based routing where appropriate
4. **Separation of Concerns**: Clear boundaries between commands, queries, and events
5. **Target Frameworks**: netstandard2.0 and net10.0

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Application Layer                         │
│  (Controllers, Application Services, API Endpoints)              │
└──────────────────────┬──────────────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────┬──────────────────────────────┐
│         Write Side (Commands)    │    Read Side (Queries)       │
├──────────────────────────────────┼──────────────────────────────┤
│  ICommandSender                  │  IQueryProcessor             │
│         ▼                        │         ▼                    │
│  Command Handlers                │  Query Handlers              │
│         ▼                        │         ▼                    │
│  ISession (Unit of Work)         │  Read Models / DTOs          │
│         ▼                        │         ▲                    │
│  IRepository                     │         │                    │
│         ▼                        │         │                    │
│  Aggregate Roots                 │  Event Handlers              │
│         ▼                        │         ▲                    │
│  Domain Events                   │         │                    │
└─────────┬────────────────────────┴─────────┼───────────────────┘
          ▼                                  │
   ┌─────────────┐                           │
   │ IEventStore │───────────────────────────┘
   │   Save()    │      IEventPublisher
   │   Get()     │      Publish()
   └─────────────┘
```

## Core Concepts

### Message Hierarchy

All messages in CQRSlite inherit from `IMessage`:

```csharp
IMessage (marker interface)
├── ICommand        // Imperative: "Create", "Update", "Delete"
├── IEvent          // Past tense: "Created", "Updated", "Deleted"
└── IQuery<TReturn> // Query with return type
```

### Commands

Commands represent **intentions to change state**. They are imperative and should have exactly **one handler**.

```csharp
public class CreateInventoryItem : ICommand
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

public class InventoryCommandHandler : ICommandHandler<CreateInventoryItem>
{
    private readonly ISession _session;

    public InventoryCommandHandler(ISession session)
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

**Key Points:**
- Commands are sent via `ICommandSender`
- Exactly one handler per command type
- Use `ISession` to track aggregates
- Call `Commit()` to persist changes

### Events

Events represent **facts that have occurred**. They are past tense and can have **zero or more handlers**.

```csharp
public class InventoryItemCreated : IEvent
{
    public Guid Id { get; set; }
    public int Version { get; set; }
    public DateTimeOffset TimeStamp { get; set; }
    public string Name { get; set; }
}

public class InventoryItemDetailView : ICancellableEventHandler<InventoryItemCreated>
{
    public Task Handle(InventoryItemCreated message, CancellationToken token)
    {
        // Update read model
        _database.Add(message.Id, new InventoryItemDto
        {
            Id = message.Id,
            Name = message.Name
        });
        return Task.CompletedTask;
    }
}
```

**Key Points:**
- Events are published via `IEventPublisher`
- Multiple handlers can subscribe to the same event
- All handlers run in parallel via `Task.WhenAll`
- Events should be immutable and contain all necessary data

### Queries

Queries represent **requests for information**. They should not modify state.

```csharp
public class GetInventoryItemDetails : IQuery<InventoryItemDetailsDto>
{
    public Guid Id { get; set; }
}

public class InventoryItemDetailsHandler :
    IQueryHandler<GetInventoryItemDetails, InventoryItemDetailsDto>
{
    public Task<InventoryItemDetailsDto> Handle(GetInventoryItemDetails query)
    {
        return Task.FromResult(_database[query.Id]);
    }
}
```

**Key Points:**
- Queries are processed via `IQueryProcessor`
- Exactly one handler per query type
- Queries return typed results
- Read models are optimized for queries

### Aggregates

Aggregates are consistency boundaries in your domain. They inherit from `AggregateRoot` and use event sourcing.

```csharp
public class InventoryItem : AggregateRoot
{
    private bool _activated;
    private string _name;

    // Constructor for new aggregates
    public InventoryItem(Guid id, string name)
    {
        Id = id;
        ApplyChange(new InventoryItemCreated(id, name));
    }

    // Required parameterless constructor for rehydration
    private InventoryItem() { }

    // Business logic methods
    public void ChangeName(string newName)
    {
        if (!_activated)
            throw new InvalidOperationException("Item is deactivated");

        ApplyChange(new InventoryItemRenamed(Id, newName));
    }

    public void Deactivate()
    {
        if (!_activated)
            throw new InvalidOperationException("Already deactivated");

        ApplyChange(new InventoryItemDeactivated(Id));
    }

    // Convention-based event application (private methods)
    private void Apply(InventoryItemCreated e)
    {
        _name = e.Name;
        _activated = true;
    }

    private void Apply(InventoryItemRenamed e)
    {
        _name = e.NewName;
    }

    private void Apply(InventoryItemDeactivated e)
    {
        _activated = false;
    }
}
```

**Key Points:**
- Inherit from `AggregateRoot`
- Use `ApplyChange()` for new events
- Implement `Apply(EventType)` methods for state changes
- All state changes must go through events
- Private parameterless constructor for rehydration

### Event Application Pattern

CQRSlite uses convention-based routing for event application:

1. Call `ApplyChange(event)` from business logic
2. Framework calls `ApplyEvent(event)` (virtual method)
3. `ApplyEvent` uses reflection to find `Apply(EventType)` method
4. Method is cached for performance via `DynamicInvoker`

**Performance Optimization:**
The reflection-based method invocation is cached using compiled expressions, making subsequent calls nearly as fast as direct method invocation.

## Getting Started

### 1. Installation

```bash
dotnet add package CQRSlite
```

### 2. Define Your Messages

```csharp
// Commands
public class CreateProduct : ICommand
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// Events
public class ProductCreated : IEvent
{
    public Guid Id { get; set; }
    public int Version { get; set; }
    public DateTimeOffset TimeStamp { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// Queries
public class GetProduct : IQuery<ProductDto>
{
    public Guid Id { get; set; }
}
```

### 3. Implement Your Aggregate

```csharp
public class Product : AggregateRoot
{
    private string _name;
    private decimal _price;
    private bool _discontinued;

    public Product(Guid id, string name, decimal price)
    {
        Id = id;
        ApplyChange(new ProductCreated(id, name, price));
    }

    private Product() { } // For rehydration

    public void ChangePrice(decimal newPrice)
    {
        if (_discontinued)
            throw new InvalidOperationException("Cannot change price of discontinued product");

        ApplyChange(new ProductPriceChanged(Id, newPrice));
    }

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

### 4. Implement Handlers

```csharp
// Command Handler
public class ProductCommandHandlers : ICommandHandler<CreateProduct>
{
    private readonly ISession _session;

    public ProductCommandHandlers(ISession session)
    {
        _session = session;
    }

    public async Task Handle(CreateProduct message)
    {
        var product = new Product(message.Id, message.Name, message.Price);
        await _session.Add(product);
        await _session.Commit();
    }
}

// Event Handler (for read model)
public class ProductEventHandlers : ICancellableEventHandler<ProductCreated>
{
    private readonly IReadDatabase _database;

    public async Task Handle(ProductCreated message, CancellationToken token)
    {
        await _database.Insert(new ProductDto
        {
            Id = message.Id,
            Name = message.Name,
            Price = message.Price
        });
    }
}

// Query Handler
public class ProductQueryHandlers : IQueryHandler<GetProduct, ProductDto>
{
    private readonly IReadDatabase _database;

    public Task<ProductDto> Handle(GetProduct query)
    {
        return _database.GetById(query.Id);
    }
}
```

### 5. Implement Event Store

You **must** implement `IEventStore`. Here's a minimal example:

```csharp
public class SqlEventStore : IEventStore
{
    private readonly IEventPublisher _publisher;
    private readonly IDbConnection _connection;

    public SqlEventStore(IEventPublisher publisher, IDbConnection connection)
    {
        _publisher = publisher;
        _connection = connection;
    }

    public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var @event in events)
        {
            // Serialize and save event
            await _connection.ExecuteAsync(
                "INSERT INTO Events (AggregateId, Version, Type, Data, Timestamp) VALUES (@Id, @Version, @Type, @Data, @TimeStamp)",
                new
                {
                    @event.Id,
                    @event.Version,
                    Type = @event.GetType().AssemblyQualifiedName,
                    Data = JsonSerializer.Serialize(@event),
                    @event.TimeStamp
                });

            // Publish event after saving (important for consistency)
            await _publisher.Publish(@event, cancellationToken);
        }
    }

    public async Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default)
    {
        var eventData = await _connection.QueryAsync<EventRecord>(
            "SELECT * FROM Events WHERE AggregateId = @AggregateId AND Version > @FromVersion ORDER BY Version",
            new { AggregateId = aggregateId, FromVersion = fromVersion });

        return eventData.Select(e =>
            (IEvent)JsonSerializer.Deserialize(e.Data, Type.GetType(e.Type)));
    }
}
```

### 6. Configure Dependency Injection

```csharp
var builder = WebApplication.CreateBuilder(args);

// Router (central message router)
builder.Services.AddSingleton<Router>(new Router());
builder.Services.AddSingleton<ICommandSender>(sp => sp.GetRequiredService<Router>());
builder.Services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<Router>());
builder.Services.AddSingleton<IQueryProcessor>(sp => sp.GetRequiredService<Router>());
builder.Services.AddSingleton<IHandlerRegistrar>(sp => sp.GetRequiredService<Router>());

// Event store (singleton)
builder.Services.AddSingleton<IEventStore, SqlEventStore>();

// Repository (scoped per request)
builder.Services.AddScoped<IRepository>(sp =>
    new Repository(sp.GetRequiredService<IEventStore>()));

// Session (scoped per request)
builder.Services.AddScoped<ISession, Session>();

// Register your command, event and query handler classes in DI too, e.g.
// builder.Services.AddTransient<ProductCommandHandlers>();

var app = builder.Build();

// Route messages to handlers. See Sample/CQRSWeb/Program.cs for resolving
// handlers from the current request scope.
new RouteRegistrar(app.Services).RegisterInAssemblyOf(typeof(ProductCommandHandlers));
```

### 7. Use in Controllers/Application Layer

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ICommandSender _commandSender;
    private readonly IQueryProcessor _queryProcessor;

    public ProductsController(ICommandSender commandSender, IQueryProcessor queryProcessor)
    {
        _commandSender = commandSender;
        _queryProcessor = queryProcessor;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProduct command, CancellationToken token)
    {
        await _commandSender.Send(command, token);
        return CreatedAtAction(nameof(Get), new { id = command.Id }, null);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> Get(Guid id)
    {
        var result = await _queryProcessor.Query(new GetProduct { Id = id });
        return Ok(result);
    }
}
```

## Implementation Guide

### Handling Optimistic Concurrency

CQRSlite includes built-in optimistic concurrency checking:

```csharp
public class ChangeProductPrice : ICommand
{
    public Guid Id { get; set; }
    public decimal NewPrice { get; set; }
    public int ExpectedVersion { get; set; } // Important!
}

public class ProductCommandHandler : ICommandHandler<ChangeProductPrice>
{
    private readonly ISession _session;

    public async Task Handle(ChangeProductPrice message)
    {
        // Pass expected version - throws ConcurrencyException if mismatch
        var product = await _session.Get<Product>(message.Id, message.ExpectedVersion);
        product.ChangePrice(message.NewPrice);
        await _session.Commit();
    }
}
```

**In the UI:**
Include version in the form as a hidden field:
```html
<input type="hidden" name="expectedVersion" value="@Model.Version" />
```

### Implementing Snapshots

For aggregates with many events, snapshots improve performance:

```csharp
// 1. Define snapshot class
public class ProductSnapshot : Snapshot
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool Discontinued { get; set; }
}

// 2. Change aggregate to inherit from SnapshotAggregateRoot
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

    protected override void RestoreFromSnapshot(ProductSnapshot snapshot)
    {
        _name = snapshot.Name;
        _price = snapshot.Price;
        _discontinued = snapshot.Discontinued;
    }
}

// 3. Implement ISnapshotStore
public class SqlSnapshotStore : ISnapshotStore
{
    public async Task<Snapshot?> Get(Guid id, CancellationToken cancellationToken = default)
    {
        // Retrieve and deserialize snapshot from database
    }

    public async Task Save(Snapshot snapshot, CancellationToken cancellationToken = default)
    {
        // Serialize and save snapshot to database
    }
}

// 4. Configure with SnapshotRepository
services.AddScoped<ISnapshotStore, SqlSnapshotStore>();
services.AddScoped<ISnapshotStrategy, DefaultSnapshotStrategy>(); // Snapshots every 100 events
services.AddScoped<IRepository>(sp =>
    new SnapshotRepository(
        sp.GetService<ISnapshotStore>(),
        sp.GetService<ISnapshotStrategy>(),
        new Repository(sp.GetService<IEventStore>()),
        sp.GetService<IEventStore>()));
```

### Implementing Caching

Add caching layer for frequently accessed aggregates:

```csharp
services.AddSingleton<ICache, MemoryCache>();
services.AddScoped<IRepository>(sp =>
    new CacheRepository(
        new Repository(sp.GetService<IEventStore>()),
        sp.GetService<IEventStore>(),
        sp.GetService<ICache>()));

// Or combine with snapshots:
services.AddScoped<IRepository>(sp =>
    new CacheRepository(
        new SnapshotRepository(
            sp.GetService<ISnapshotStore>(),
            sp.GetService<ISnapshotStrategy>(),
            new Repository(sp.GetService<IEventStore>()),
            sp.GetService<IEventStore>()),
        sp.GetService<IEventStore>(),
        sp.GetService<ICache>()));
```

**CacheRepository Features:**
- Thread-safe per-aggregate locking
- Applies new events to cached aggregates
- Invalidates cache if events were skipped
- Reduces load on event store

### Manual Handler Registration

Instead of automatic registration, you can register handlers manually:

```csharp
var registrar = serviceProvider.GetService<IHandlerRegistrar>();

// Register command handler
registrar.RegisterHandler<CreateProduct>(async (message, token) =>
{
    var handler = serviceProvider.GetService<ProductCommandHandlers>();
    await handler.Handle((CreateProduct)message);
});

// Register event handler
registrar.RegisterHandler<ProductCreated>(async (message, token) =>
{
    var handler = serviceProvider.GetService<ProductEventHandlers>();
    await handler.Handle((ProductCreated)message, token);
});
```

## Extension Points

### Custom Event Store

Implement `IEventStore` for your persistence layer:

```csharp
public interface IEventStore
{
    Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default);
    Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default);
}
```

**Considerations:**
- Use transactions for atomicity
- Publish events after successful save
- Handle concurrency conflicts
- Consider event versioning for schema evolution

### Custom Snapshot Strategy

Implement `ISnapshotStrategy` for custom snapshotting logic:

```csharp
public class CustomSnapshotStrategy : ISnapshotStrategy
{
    public bool IsSnapshotable(Type aggregateType)
    {
        // Only snapshot specific aggregate types
        return aggregateType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISnapshotAggregate<>));
    }

    public bool ShouldMakeSnapShot(AggregateRoot aggregate)
    {
        // Snapshot based on custom logic
        // Example: Snapshot every 50 events, or after specific event types
        return aggregate.Version % 50 == 0;
    }
}
```

### Custom Cache Implementation

Implement `ICache` for custom caching strategies:

```csharp
public class RedisCache : ICache
{
    private readonly IConnectionMultiplexer _redis;

    public bool IsTracked(Guid id) { /* ... */ }
    public void Set(Guid id, AggregateRoot aggregate) { /* ... */ }
    public AggregateRoot Get(Guid id) { /* ... */ }
    public void Remove(Guid id) { /* ... */ }
    public void RegisterEvictionCallback(Action<Guid> action) { /* ... */ }
}
```

### Custom Event Application

Override `ApplyEvent` for custom event routing:

```csharp
public class CustomAggregate : AggregateRoot
{
    protected override void ApplyEvent(IEvent @event)
    {
        // Custom routing logic
        switch (@event)
        {
            case CustomEvent1 e:
                ApplyCustomEvent1(e);
                break;
            case CustomEvent2 e:
                ApplyCustomEvent2(e);
                break;
            default:
                base.ApplyEvent(@event); // Fall back to convention-based routing
                break;
        }
    }
}
```

## Best Practices

### Aggregate Design

1. **Keep aggregates small**: Large aggregates with many events cause performance issues
2. **One aggregate per transaction**: Don't modify multiple aggregates in one command handler
3. **Protect invariants**: All business rules should be enforced in the aggregate
4. **Use meaningful events**: Events should capture business intent, not just state changes

```csharp
// Good: Business intent is clear
public class OrderPlaced : IEvent { /* ... */ }
public class OrderShipped : IEvent { /* ... */ }

// Bad: Generic state change
public class OrderStateChanged : IEvent
{
    public string NewState { get; set; }
}
```

### Event Design

1. **Events are immutable**: Never change event structure after deployment
2. **Events are facts**: Past tense, describe what happened
3. **Include all necessary data**: Events should be self-contained
4. **Consider versioning**: Plan for event schema evolution

```csharp
// Good: Self-contained event
public class OrderPlaced : IEvent
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; }
    public List<OrderLine> Lines { get; set; }
    public decimal TotalAmount { get; set; }
    // ... all necessary data
}

// Bad: Missing data
public class OrderPlaced : IEvent
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; } // Missing customer details
}
```

### Command Handlers

1. **One purpose**: Each command should do one thing
2. **Validate in aggregate**: Business validation belongs in the aggregate
3. **Technical validation in handler**: Check permissions, external dependencies
4. **Use ISession**: Always commit through session for consistency

```csharp
public async Task Handle(PlaceOrder command)
{
    // Technical validation
    if (!await _permissions.CanPlaceOrder(command.CustomerId))
        throw new UnauthorizedAccessException();

    // Load aggregate
    var customer = await _session.Get<Customer>(command.CustomerId);

    // Business logic (validation happens in aggregate)
    var order = customer.PlaceOrder(command.Lines);

    // Track and commit
    await _session.Add(order);
    await _session.Commit();
}
```

### Read Models

1. **Denormalized**: Optimize for queries, not normalization
2. **Eventually consistent**: Accept that reads may lag slightly
3. **Multiple read models**: Different models for different use cases
4. **Cache when appropriate**: Use caching for expensive queries

### Error Handling

1. **Business rule violations**: Throw exceptions from aggregates
2. **Concurrency**: Catch `ConcurrencyException` and retry or inform user
3. **Event handler failures**: Consider compensation or dead letter queue
4. **Idempotency**: Make event handlers idempotent where possible

### Testing

1. **Test aggregates in isolation**: No dependencies needed
2. **Test event application**: Verify state changes correctly
3. **Test command handlers**: Mock ISession
4. **Test event handlers**: Verify read model updates
5. **Integration tests**: Test full flow with real event store

Example aggregate test:

```csharp
[Fact]
public void ChangingPrice_RaisesProductPriceChangedEvent()
{
    // Arrange
    var product = new Product(Guid.NewGuid(), "Test", 100m);
    product.FlushUncommittedChanges(); // Clear creation event

    // Act
    product.ChangePrice(150m);

    // Assert
    var events = product.FlushUncommittedChanges();
    Assert.Single(events);
    Assert.IsType<ProductPriceChanged>(events.First());
    Assert.Equal(150m, ((ProductPriceChanged)events.First()).NewPrice);
}
```

## Testing

### Unit Testing Aggregates

Aggregates are easy to test in isolation:

```csharp
public class ProductTests
{
    [Fact]
    public void CreatingProduct_RaisesProductCreatedEvent()
    {
        var id = Guid.NewGuid();
        var product = new Product(id, "Test Product", 99.99m);

        var events = product.FlushUncommittedChanges();

        Assert.Single(events);
        var createdEvent = Assert.IsType<ProductCreated>(events.First());
        Assert.Equal(id, createdEvent.Id);
        Assert.Equal("Test Product", createdEvent.Name);
        Assert.Equal(99.99m, createdEvent.Price);
    }

    [Fact]
    public void ChangingPrice_UpdatesPrice()
    {
        var product = new Product(Guid.NewGuid(), "Test", 100m);
        product.FlushUncommittedChanges();

        product.ChangePrice(150m);

        var events = product.FlushUncommittedChanges();
        Assert.Single(events);
        Assert.IsType<ProductPriceChanged>(events.First());
    }

    [Fact]
    public void ChangingPriceOfDiscontinuedProduct_ThrowsException()
    {
        var product = new Product(Guid.NewGuid(), "Test", 100m);
        product.Discontinue();

        Assert.Throws<InvalidOperationException>(() => product.ChangePrice(150m));
    }
}
```

### Unit Testing Command Handlers

Mock `ISession` for command handler tests:

```csharp
public class ProductCommandHandlerTests
{
    [Fact]
    public async Task CreateProduct_CreatesNewProduct()
    {
        var mockSession = new Mock<ISession>();
        var handler = new ProductCommandHandlers(mockSession.Object);
        var command = new CreateProduct
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Price = 100m
        };

        await handler.Handle(command);

        mockSession.Verify(s => s.Add(It.IsAny<Product>()), Times.Once);
        mockSession.Verify(s => s.Commit(), Times.Once);
    }
}
```

### Integration Testing

Test the full flow with a real or in-memory event store:

```csharp
public class ProductIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProductIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateAndRetrieveProduct_WorksEndToEnd()
    {
        var client = _factory.CreateClient();
        var productId = Guid.NewGuid();

        // Create product
        var createCommand = new CreateProduct
        {
            Id = productId,
            Name = "Test Product",
            Price = 99.99m
        };
        var createResponse = await client.PostAsJsonAsync("/api/products", createCommand);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        // Wait for eventual consistency
        await Task.Delay(100);

        // Retrieve product
        var getResponse = await client.GetAsync($"/api/products/{productId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var product = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.Equal("Test Product", product.Name);
        Assert.Equal(99.99m, product.Price);
    }
}
```

## Performance Considerations

### Event Replay Performance

**Problem**: Loading aggregates with thousands of events is slow.

**Solutions:**

1. **Snapshots**: Use `SnapshotRepository` to save aggregate state periodically
   - Default strategy: Every 100 events
   - Reduces event replay significantly

2. **Aggregate Size**: Keep aggregates small
   - If an aggregate grows too large, consider splitting it
   - Use aggregate references instead of nested aggregates

3. **Caching**: Use `CacheRepository` for frequently accessed aggregates
   - Thread-safe caching layer
   - Applies new events to cached aggregates

### Handler Registration Performance

**Problem**: Reflection-based handler registration can be slow at startup.

**Solutions:**

1. **Manual Registration**: Register handlers manually for critical paths
2. **Assembly Scanning**: Limit assembly scanning to specific namespaces
3. **Startup Warmup**: Pre-register handlers during app initialization

### Event Application Performance

**Problem**: Reflection-based `Apply()` method invocation.

**Solution**: CQRSlite already handles this via `DynamicInvoker`
- Methods are found via reflection once
- Compiled into cached delegates
- Subsequent calls are nearly as fast as direct invocation

### Database Performance

1. **Event Store**:
   - Index on `AggregateId` and `Version`
   - Consider partitioning for large datasets
   - Use append-only writes (no updates/deletes)

2. **Read Models**:
   - Optimize indexes for query patterns
   - Consider separate databases for read/write
   - Use caching for expensive queries

## Contributing

### Code Style

- Follow C# coding conventions
- Use meaningful names for commands, events, and queries
- Keep methods small and focused
- Add XML documentation for public APIs

### Pull Request Process

1. Fork the repository
2. Create a feature branch
3. Write tests for new functionality
4. Ensure all tests pass
5. Update documentation
6. Submit pull request

### Testing Requirements

- All new features must have unit tests
- Integration tests for complex scenarios
- Maintain or improve code coverage

### Documentation

- Update README.md for user-facing changes
- Update DEVELOPER.md for architectural changes
- Add XML comments for public APIs
- Include code examples for new features

## Additional Resources

### External Articles

- [CQRS: A Cross-Examination of How It Works](https://www.codeproject.com/articles/991648/cqrs-a-cross-examination-of-how-it-works)
- [Real-World CQRS ES with ASP.NET and Redis](https://exceptionnotfound.net/real-world-cqrs-es-with-asp-net-and-redis-part-1-overview/)

### Sample Project

The `Sample` folder contains a complete working example:
- **CQRSCode**: Domain logic, handlers, and in-memory implementations
- **CQRSWeb**: ASP.NET Core web application
- **CQRSTest**: Tests for the sample

### Related Patterns

- **Event Sourcing**: All state changes are stored as events
- **CQRS**: Separate models for reading and writing data
- **Domain-Driven Design**: Aggregates, entities, value objects
- **Saga Pattern**: Long-running transactions across aggregates
- **Repository Pattern**: Abstraction for data access

## Frequently Asked Questions

### Why do I need to implement my own EventStore?

CQRSlite is a framework, not a complete solution. Event storage requirements vary greatly:
- SQL vs NoSQL
- Cloud vs on-premises
- Performance requirements
- Transaction guarantees
- Event schema evolution strategies

Providing a default implementation would impose unnecessary constraints.

### Should events be published before or after saving?

**After saving** is recommended. The event store's `Save` method should:
1. Persist events to storage (within transaction if possible)
2. Publish events after successful save
3. This ensures consistency and prevents publishing events that might fail to persist

### How do I handle event schema evolution?

Several strategies:
1. **Upcasting**: Convert old events to new format when loading
2. **Versioned Events**: `ProductCreatedV1`, `ProductCreatedV2`
3. **Event Transformation**: Background process to migrate old events
4. **Weak Schema**: Use optional properties with defaults

### Can I modify multiple aggregates in one command?

**No, avoid this.** Each command should modify one aggregate. If you need to coordinate changes across aggregates:
1. Use a saga or process manager
2. Emit events from first aggregate
3. Have event handlers trigger commands for other aggregates

### How do I query across aggregates?

Use **read models** (projections) that are built from events:
1. Event handlers update denormalized read models
2. Query handlers read from these optimized models
3. Read models can join data across multiple aggregates

### What about transactions?

- **Write side**: Each aggregate is a transaction boundary
- **Read side**: Eventually consistent
- **Event Store**: Use database transactions when saving events
- **Cross-aggregate**: Use sagas for distributed transactions

---

## License

Copyright 2020 Gaute Magnussen

Licensed under the Apache License, Version 2.0

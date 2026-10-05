# CQRSlite Quick Reference

A quick reference guide for common CQRSlite patterns and code snippets.

## Table of Contents
- [Message Definitions](#message-definitions)
- [Aggregate Patterns](#aggregate-patterns)
- [Handler Patterns](#handler-patterns)
- [Dependency Injection Setup](#dependency-injection-setup)
- [Common Scenarios](#common-scenarios)
- [Error Handling](#error-handling)

## Message Definitions

### Command
```csharp
public class CreateProduct : ICommand
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

### Command with Concurrency
```csharp
public class UpdateProductPrice : ICommand
{
    public Guid Id { get; set; }
    public decimal NewPrice { get; set; }
    public int ExpectedVersion { get; set; }
}
```

### Event
```csharp
public class ProductCreated : IEvent
{
    public Guid Id { get; set; }
    public int Version { get; set; }
    public DateTimeOffset TimeStamp { get; set; }

    // Business data
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

### Query
```csharp
public class GetProduct : IQuery<ProductDto>
{
    public Guid Id { get; set; }
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Version { get; set; }
}
```

## Aggregate Patterns

### Basic Aggregate
```csharp
public class Product : AggregateRoot
{
    private string _name;
    private decimal _price;
    private bool _discontinued;

    // Constructor for new aggregates
    public Product(Guid id, string name, decimal price)
    {
        Id = id;
        ApplyChange(new ProductCreated(id, name, price));
    }

    // Required parameterless constructor for rehydration
    private Product() { }

    // Business logic method
    public void ChangePrice(decimal newPrice)
    {
        if (_discontinued)
            throw new InvalidOperationException("Cannot change price of discontinued product");
        if (newPrice < 0)
            throw new ArgumentException("Price cannot be negative");

        ApplyChange(new ProductPriceChanged(Id, newPrice));
    }

    // Convention-based event application
    private void Apply(ProductCreated e)
    {
        _name = e.Name;
        _price = e.Price;
        _discontinued = false;
    }

    private void Apply(ProductPriceChanged e)
    {
        _price = e.NewPrice;
    }

    private void Apply(ProductDiscontinued e)
    {
        _discontinued = true;
    }
}
```

### Snapshot Aggregate
```csharp
public class ProductSnapshot : Snapshot
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool Discontinued { get; set; }
}

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

    // Rest of aggregate implementation...
}
```

## Handler Patterns

### Command Handler
```csharp
public class ProductCommandHandlers :
    ICommandHandler<CreateProduct>,
    ICommandHandler<UpdateProductPrice>
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

    public async Task Handle(UpdateProductPrice message)
    {
        var product = await _session.Get<Product>(message.Id, message.ExpectedVersion);
        product.ChangePrice(message.NewPrice);
        await _session.Commit();
    }
}
```

### Command Handler with Cancellation
```csharp
public class ProductCommandHandlers : ICancellableCommandHandler<CreateProduct>
{
    private readonly ISession _session;

    public async Task Handle(CreateProduct message, CancellationToken token)
    {
        var product = new Product(message.Id, message.Name, message.Price);
        await _session.Add(product, token);
        await _session.Commit(token);
    }
}
```

### Event Handler (Read Model)
```csharp
public class ProductListView : ICancellableEventHandler<ProductCreated>,
                                ICancellableEventHandler<ProductPriceChanged>,
                                ICancellableEventHandler<ProductDiscontinued>
{
    private readonly IReadDatabase _database;

    public ProductListView(IReadDatabase database)
    {
        _database = database;
    }

    public async Task Handle(ProductCreated e, CancellationToken token)
    {
        await _database.Insert(new ProductListItemDto
        {
            Id = e.Id,
            Name = e.Name,
            Price = e.Price,
            Version = e.Version
        }, token);
    }

    public async Task Handle(ProductPriceChanged e, CancellationToken token)
    {
        var item = await _database.Get(e.Id, token);
        item.Price = e.NewPrice;
        item.Version = e.Version;
        await _database.Update(item, token);
    }

    public async Task Handle(ProductDiscontinued e, CancellationToken token)
    {
        await _database.Delete(e.Id, token);
    }
}
```

### Query Handler
```csharp
public class ProductQueryHandlers :
    IQueryHandler<GetProduct, ProductDto>,
    IQueryHandler<GetAllProducts, List<ProductListItemDto>>
{
    private readonly IReadDatabase _database;

    public ProductQueryHandlers(IReadDatabase database)
    {
        _database = database;
    }

    public async Task<ProductDto> Handle(GetProduct query)
    {
        return await _database.GetById(query.Id);
    }

    public async Task<List<ProductListItemDto>> Handle(GetAllProducts query)
    {
        return await _database.GetAll();
    }
}
```

## Dependency Injection Setup

### Minimal Setup
```csharp
var builder = WebApplication.CreateBuilder(args);

// Router (central hub)
var router = new Router();
builder.Services.AddSingleton(router);
builder.Services.AddSingleton<ICommandSender>(router);
builder.Services.AddSingleton<IEventPublisher>(router);
builder.Services.AddSingleton<IQueryProcessor>(router);
builder.Services.AddSingleton<IHandlerRegistrar>(router);

// Event store (you must implement)
builder.Services.AddSingleton<IEventStore, YourEventStore>();

// Repository
builder.Services.AddScoped<IRepository>(sp =>
    new Repository(sp.GetRequiredService<IEventStore>()));

// Session
builder.Services.AddScoped<ISession, Session>();

// Register your command, event and query handler classes in DI too, e.g.
// builder.Services.AddTransient<ProductCommandHandlers>();

var app = builder.Build();

// Route messages to handlers. See Sample/CQRSWeb/Program.cs for resolving
// handlers from the current request scope.
new RouteRegistrar(app.Services).RegisterInAssemblyOf(typeof(ProductCommandHandlers));
```

### Setup with Caching
```csharp
var builder = WebApplication.CreateBuilder(args);

// Router
var router = new Router();
builder.Services.AddSingleton(router);
builder.Services.AddSingleton<ICommandSender>(router);
builder.Services.AddSingleton<IEventPublisher>(router);
builder.Services.AddSingleton<IQueryProcessor>(router);
builder.Services.AddSingleton<IHandlerRegistrar>(router);

// Event store
builder.Services.AddSingleton<IEventStore, YourEventStore>();

// Cache
builder.Services.AddSingleton<ICache, MemoryCache>();

// Repository with caching
builder.Services.AddScoped<IRepository>(sp =>
    new CacheRepository(
        new Repository(sp.GetRequiredService<IEventStore>()),
        sp.GetRequiredService<IEventStore>(),
        sp.GetRequiredService<ICache>()));

// Session
builder.Services.AddScoped<ISession, Session>();

// Register your command, event and query handler classes in DI too, e.g.
// builder.Services.AddTransient<ProductCommandHandlers>();

var app = builder.Build();

// Route messages to handlers. See Sample/CQRSWeb/Program.cs for resolving
// handlers from the current request scope.
new RouteRegistrar(app.Services).RegisterInAssemblyOf(typeof(ProductCommandHandlers));
```

### Setup with Snapshotting and Caching
```csharp
var builder = WebApplication.CreateBuilder(args);

// Router
var router = new Router();
builder.Services.AddSingleton(router);
builder.Services.AddSingleton<ICommandSender>(router);
builder.Services.AddSingleton<IEventPublisher>(router);
builder.Services.AddSingleton<IQueryProcessor>(router);
builder.Services.AddSingleton<IHandlerRegistrar>(router);

// Event store
builder.Services.AddSingleton<IEventStore, YourEventStore>();

// Snapshot support
builder.Services.AddSingleton<ISnapshotStore, YourSnapshotStore>();
builder.Services.AddSingleton<ISnapshotStrategy, DefaultSnapshotStrategy>();

// Cache
builder.Services.AddSingleton<ICache, MemoryCache>();

// Repository with all decorators
builder.Services.AddScoped<IRepository>(sp =>
    new CacheRepository(
        new SnapshotRepository(
            sp.GetRequiredService<ISnapshotStore>(),
            sp.GetRequiredService<ISnapshotStrategy>(),
            new Repository(sp.GetRequiredService<IEventStore>()),
            sp.GetRequiredService<IEventStore>()),
        sp.GetRequiredService<IEventStore>(),
        sp.GetRequiredService<ICache>()));

// Session
builder.Services.AddScoped<ISession, Session>();

// Register your command, event and query handler classes in DI too, e.g.
// builder.Services.AddTransient<ProductCommandHandlers>();

var app = builder.Build();

// Route messages to handlers. See Sample/CQRSWeb/Program.cs for resolving
// handlers from the current request scope.
new RouteRegistrar(app.Services).RegisterInAssemblyOf(typeof(ProductCommandHandlers));
```

## Common Scenarios

### Sending a Command
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ICommandSender _commandSender;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProduct command, CancellationToken token)
    {
        await _commandSender.Send(command, token);
        return CreatedAtAction(nameof(Get), new { id = command.Id }, null);
    }
}
```

### Sending a Command with Concurrency Check
```csharp
[HttpPut("{id}/price")]
public async Task<IActionResult> UpdatePrice(
    Guid id,
    [FromBody] UpdatePriceRequest request,
    CancellationToken token)
{
    try
    {
        await _commandSender.Send(new UpdateProductPrice
        {
            Id = id,
            NewPrice = request.NewPrice,
            ExpectedVersion = request.Version
        }, token);

        return Ok();
    }
    catch (ConcurrencyException ex)
    {
        return Conflict(new
        {
            message = "Product was modified by another user",
            expectedVersion = ex.ExpectedVersion,
            actualVersion = ex.ActualVersion
        });
    }
}
```

### Processing a Query
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<ProductDto>> Get(Guid id)
{
    try
    {
        var result = await _queryProcessor.Query(new GetProduct { Id = id });
        return Ok(result);
    }
    catch (KeyNotFoundException)
    {
        return NotFound();
    }
}
```

### Using Session (Multiple Operations)
```csharp
public async Task Handle(TransferInventory command)
{
    // Get both aggregates in the same session
    var source = await _session.Get<InventoryItem>(command.SourceId);
    var destination = await _session.Get<InventoryItem>(command.DestinationId);

    // Perform operations
    source.Remove(command.Quantity);
    destination.Add(command.Quantity);

    // Commit saves both aggregates
    await _session.Commit();
}
```

### Direct Repository Usage
```csharp
public async Task Handle(CreateProduct command)
{
    var product = new Product(command.Id, command.Name, command.Price);
    await _repository.Save(product);
}

public async Task Handle(UpdateProduct command)
{
    var product = await _repository.Get<Product>(command.Id);
    product.Update(command.Name, command.Price);
    await _repository.Save(product, command.ExpectedVersion);
}
```

## Event Store Implementation

### SQL Event Store Example
```csharp
public class SqlEventStore : IEventStore
{
    private readonly IDbConnection _connection;
    private readonly IEventPublisher _publisher;

    public SqlEventStore(IDbConnection connection, IEventPublisher publisher)
    {
        _connection = connection;
        _publisher = publisher;
    }

    public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        using var transaction = _connection.BeginTransaction();

        try
        {
            foreach (var @event in events)
            {
                // Serialize and save
                await _connection.ExecuteAsync(
                    "INSERT INTO Events (AggregateId, Version, Type, Data, Timestamp) " +
                    "VALUES (@Id, @Version, @Type, @Data, @TimeStamp)",
                    new
                    {
                        @event.Id,
                        @event.Version,
                        Type = @event.GetType().AssemblyQualifiedName,
                        Data = JsonSerializer.Serialize(@event, @event.GetType()),
                        @event.TimeStamp
                    },
                    transaction);

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

    public async Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default)
    {
        var records = await _connection.QueryAsync<EventRecord>(
            "SELECT * FROM Events WHERE AggregateId = @AggregateId AND Version > @FromVersion ORDER BY Version",
            new { AggregateId = aggregateId, FromVersion = fromVersion });

        return records.Select(r =>
        {
            var type = Type.GetType(r.Type);
            return (IEvent)JsonSerializer.Deserialize(r.Data, type);
        });
    }
}
```

### In-Memory Event Store (Testing)
```csharp
public class InMemoryEventStore : IEventStore
{
    private readonly IEventPublisher _publisher;
    private readonly Dictionary<Guid, List<IEvent>> _events = new();

    public InMemoryEventStore(IEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var @event in events)
        {
            if (!_events.ContainsKey(@event.Id))
                _events[@event.Id] = new List<IEvent>();

            _events[@event.Id].Add(@event);

            await _publisher.Publish(@event, cancellationToken);
        }
    }

    public Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default)
    {
        if (!_events.ContainsKey(aggregateId))
            return Task.FromResult(Enumerable.Empty<IEvent>());

        var events = _events[aggregateId]
            .Where(e => e.Version > fromVersion)
            .OrderBy(e => e.Version);

        return Task.FromResult(events);
    }
}
```

## Error Handling

### Handling Concurrency Exceptions
```csharp
public async Task<IActionResult> UpdateProduct(UpdateProduct command)
{
    try
    {
        await _commandSender.Send(command);
        return Ok();
    }
    catch (ConcurrencyException ex)
    {
        return Conflict(new
        {
            message = "Resource was modified by another user",
            resourceId = ex.Id,
            expectedVersion = ex.ExpectedVersion,
            actualVersion = ex.ActualVersion
        });
    }
}
```

### Handling Aggregate Not Found
```csharp
public async Task<IActionResult> GetProduct(Guid id)
{
    try
    {
        var result = await _queryProcessor.Query(new GetProduct { Id = id });
        return Ok(result);
    }
    catch (AggregateNotFoundException ex)
    {
        return NotFound(new { message = $"Product {ex.Id} not found" });
    }
}
```

### Handling Business Rule Violations
```csharp
public async Task<IActionResult> UpdatePrice(UpdateProductPrice command)
{
    try
    {
        await _commandSender.Send(command);
        return Ok();
    }
    catch (InvalidOperationException ex)
    {
        // Business rule violation from aggregate
        return BadRequest(new { message = ex.Message });
    }
    catch (ArgumentException ex)
    {
        // Validation error
        return BadRequest(new { message = ex.Message });
    }
}
```

### Retry on Concurrency Conflict
```csharp
public async Task Handle(UpdateProduct command, int maxRetries = 3)
{
    for (int attempt = 0; attempt < maxRetries; attempt++)
    {
        try
        {
            var product = await _repository.Get<Product>(command.Id);
            product.Update(command.Name, command.Price);
            await _repository.Save(product, product.Version);
            return;
        }
        catch (ConcurrencyException) when (attempt < maxRetries - 1)
        {
            // Retry on conflict
            await Task.Delay(100 * (attempt + 1)); // Exponential backoff
        }
    }

    throw new InvalidOperationException("Failed to update product after multiple retries");
}
```

## Testing Patterns

### Testing Aggregates
```csharp
[Fact]
public void ChangingPrice_RaisesCorrectEvent()
{
    // Arrange
    var product = new Product(Guid.NewGuid(), "Test", 100m);
    product.FlushUncommittedChanges(); // Clear creation event

    // Act
    product.ChangePrice(150m);

    // Assert
    var events = product.FlushUncommittedChanges();
    Assert.Single(events);
    var priceChangedEvent = Assert.IsType<ProductPriceChanged>(events.First());
    Assert.Equal(150m, priceChangedEvent.NewPrice);
}

[Fact]
public void ChangingPriceOfDiscontinuedProduct_ThrowsException()
{
    // Arrange
    var product = new Product(Guid.NewGuid(), "Test", 100m);
    product.Discontinue();

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => product.ChangePrice(150m));
}
```

### Testing Command Handlers
```csharp
[Fact]
public async Task CreateProduct_AddsProductToSession()
{
    // Arrange
    var mockSession = new Mock<ISession>();
    var handler = new ProductCommandHandlers(mockSession.Object);
    var command = new CreateProduct { Id = Guid.NewGuid(), Name = "Test", Price = 100m };

    // Act
    await handler.Handle(command);

    // Assert
    mockSession.Verify(s => s.Add(It.IsAny<Product>()), Times.Once);
    mockSession.Verify(s => s.Commit(), Times.Once);
}
```

### Testing Event Handlers
```csharp
[Fact]
public async Task ProductCreated_AddsToReadModel()
{
    // Arrange
    var mockDatabase = new Mock<IReadDatabase>();
    var handler = new ProductListView(mockDatabase.Object);
    var @event = new ProductCreated
    {
        Id = Guid.NewGuid(),
        Name = "Test",
        Price = 100m,
        Version = 1,
        TimeStamp = DateTimeOffset.UtcNow
    };

    // Act
    await handler.Handle(@event, CancellationToken.None);

    // Assert
    mockDatabase.Verify(db => db.Insert(
        It.Is<ProductListItemDto>(p => p.Name == "Test" && p.Price == 100m),
        It.IsAny<CancellationToken>()), Times.Once);
}
```

## Performance Tips

1. **Use Snapshots** for aggregates with many events (>100)
2. **Enable Caching** for frequently accessed aggregates
3. **Use CancellationToken** to support request cancellation
4. **Denormalize Read Models** for optimal query performance
5. **Index Event Store** on AggregateId and Version
6. **Keep Aggregates Small** - split if they grow too large
7. **Use Async/Await** throughout for better scalability

## Common Mistakes

1. **Forgetting parameterless constructor** on aggregates
2. **Not committing session** after making changes
3. **Modifying state without events** in aggregates
4. **Publishing events before saving** (should be after)
5. **Using public Apply methods** (should be private)
6. **Not handling ConcurrencyException** when updating
7. **Querying from write model** (use read models instead)

---

For detailed explanations and advanced scenarios, see [DEVELOPER.md](./DEVELOPER.md) and [API_REFERENCE.md](./API_REFERENCE.md).

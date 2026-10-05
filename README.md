# CQRSlite

[![Build](https://github.com/gautema/cqrslite/actions/workflows/ci.yml/badge.svg)](https://github.com/gautema/cqrslite/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/vpre/cqrslite.svg)](https://www.nuget.org/packages/cqrslite)

A lightweight CQRS and Event Sourcing framework for .NET

## Overview

CQRSlite is a small, focused CQRS (Command Query Responsibility Segregation) and Event Sourcing framework for .NET. It provides the essential building blocks for implementing CQRS/ES patterns while maintaining flexibility and pluggability.

**Key Characteristics:**
- Minimal dependencies (only Microsoft.Extensions.Caching.Memory)
- Targets netstandard2.0 and net10.0
- Convention-based event application with performance optimization
- Pluggable architecture - replace any component with custom implementations
- Thread-safe caching and repository decorators

CQRSlite originated as a CQRS sample project by Greg Young and Gaute Magnussen in 2010. Original code: http://github.com/gregoryyoung/m-r

## Features

- **Command Sending** - Dispatch commands to handlers with 1:1 routing
- **Event Publishing** - Publish events to multiple handlers (1:N routing)
- **Query Processing** - Process queries with typed results
- **Unit of Work** - Session-based aggregate tracking for consistency
- **Repository Pattern** - Get and save aggregates with event sourcing
- **Optimistic Concurrency** - Built-in concurrency checking and conflict detection
- **Message Router** - Automatic handler registration via reflection
- **Snapshotting** - Performance optimization for aggregates with many events
- **Caching** - Thread-safe caching layer with automatic invalidation

## Quick Start

### Installation

```bash
dotnet add package CQRSlite
```

### Basic Usage

1. **Define your messages:**

```csharp
// Command
public class CreateProduct : ICommand
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// Event
public class ProductCreated : IEvent
{
    public Guid Id { get; set; }
    public int Version { get; set; }
    public DateTimeOffset TimeStamp { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

2. **Create your aggregate:**

```csharp
public class Product : AggregateRoot
{
    private string _name;
    private decimal _price;

    public Product(Guid id, string name, decimal price)
    {
        Id = id;
        ApplyChange(new ProductCreated { Id = id, Name = name, Price = price });
    }

    private Product() { } // Required for rehydration

    private void Apply(ProductCreated e)
    {
        _name = e.Name;
        _price = e.Price;
    }
}
```

3. **Implement handlers:**

```csharp
public class ProductCommandHandler : ICommandHandler<CreateProduct>
{
    private readonly ISession _session;

    public ProductCommandHandler(ISession session)
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
```

4. **Configure services:**

```csharp
using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Events;
using CQRSlite.Queries;
using CQRSlite.Routing;
using ISession = CQRSlite.Domain.ISession; // Avoid clash with Microsoft.AspNetCore.Http.ISession

var builder = WebApplication.CreateBuilder(args);

// Register Router
var router = new Router();
builder.Services.AddSingleton(router);
builder.Services.AddSingleton<ICommandSender>(router);
builder.Services.AddSingleton<IEventPublisher>(router);
builder.Services.AddSingleton<IQueryProcessor>(router);
builder.Services.AddSingleton<IHandlerRegistrar>(router);

// Register core services
builder.Services.AddSingleton<IEventStore, YourEventStore>(); // You must implement this
builder.Services.AddScoped<IRepository>(sp => new Repository(sp.GetRequiredService<IEventStore>()));
builder.Services.AddScoped<ISession, Session>();

// Every handler class must be resolvable from DI (the sample scans for them with Scrutor)
builder.Services.AddTransient<ProductCommandHandler>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Route messages to handlers, resolving them from the current request scope
new RouteRegistrar(new RequestServiceProvider(app.Services))
    .RegisterInAssemblyOf(typeof(ProductCommandHandler));

app.Run();

/// <summary>
/// Resolves services from the current request scope when there is one, so scoped
/// services like ISession are shared between a request and the handlers it triggers.
/// </summary>
internal class RequestServiceProvider(IServiceProvider services) : IServiceProvider
{
    private readonly IHttpContextAccessor? _contextAccessor = services.GetService<IHttpContextAccessor>();

    public object? GetService(Type serviceType) =>
        _contextAccessor?.HttpContext?.RequestServices.GetService(serviceType) ??
        services.GetService(serviceType);
}
```

Handlers are resolved when a message is routed, so don't build `RouteRegistrar` on `app.Services` directly: that resolves scoped services such as `ISession` from the root provider.

## Documentation

- **[Developer Documentation](https://github.com/gautema/cqrslite/blob/master/DEVELOPER.md)** - Comprehensive guide covering architecture, implementation patterns, best practices, and testing
- **[API Reference](https://github.com/gautema/cqrslite/blob/master/API_REFERENCE.md)** - Complete API documentation for all interfaces and classes
- **[Sample Project](https://github.com/gautema/cqrslite/tree/master/Sample)** - Working example demonstrating common usage patterns

## External Resources

Great introductions to CQRS and CQRSlite:
- [CQRS: A Cross-Examination of How It Works](https://www.codeproject.com/articles/991648/cqrs-a-cross-examination-of-how-it-works)
- [Real-World CQRS ES with ASP.NET and Redis](https://exceptionnotfound.net/real-world-cqrs-es-with-asp-net-and-redis-part-1-overview/)

## Requirements

You **must** implement your own `IEventStore` for persistence. CQRSlite provides the framework but intentionally does not include a default event store implementation, as storage requirements vary greatly between applications.

Example event stores:
- SQL Server / PostgreSQL / MySQL
- NoSQL databases (MongoDB, CosmosDB)
- Event Store DB
- Azure Table Storage
- In-memory (for testing, included in sample)

See [DEVELOPER.md](https://github.com/gautema/cqrslite/blob/master/DEVELOPER.md#5-implement-event-store) for implementation guidance.

## Architecture

CQRSlite follows clean CQRS/ES principles:

```
Application Layer
    ↓
Commands → CommandHandlers → Aggregates → Events → EventStore
    ↓                                          ↓
Queries → QueryHandlers → ReadModels ← EventHandlers
```

**Write Side (Commands):**
- Commands express intent to change state
- Aggregates enforce business rules
- Events record what happened
- Event store persists events

**Read Side (Queries):**
- Events update denormalized read models
- Queries read from optimized projections
- Eventually consistent with write side

## Contributing

Contributions are welcome! Please see [DEVELOPER.md](https://github.com/gautema/cqrslite/blob/master/DEVELOPER.md#contributing) for guidelines.

## Version Compatibility

- **netstandard2.0** - Compatible with .NET Framework 4.6.2+ and .NET Core 2.0+ / .NET 5+
- **net10.0** - Latest .NET features and performance improvements

## License
Copyright 2020 Gaute Magnussen

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

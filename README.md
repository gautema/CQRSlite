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
        ApplyChange(new ProductCreated(id, name, price));
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
// Register Router
var router = new Router();
services.AddSingleton(router);
services.AddSingleton<ICommandSender>(router);
services.AddSingleton<IEventPublisher>(router);
services.AddSingleton<IHandlerRegistrar>(router);

// Register core services
services.AddSingleton<IEventStore, YourEventStore>(); // You must implement this
services.AddScoped<IRepository>(sp => new Repository(sp.GetService<IEventStore>()));
services.AddScoped<ISession, Session>();

// Auto-register handlers
var registrar = new RouteRegistrar(serviceProvider);
registrar.Register(typeof(ProductCommandHandler).Assembly);
```

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

See [DEVELOPER.md](https://github.com/gautema/cqrslite/blob/master/DEVELOPER.md#implementing-event-store) for implementation guidance.

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

- **netstandard2.0** - Compatible with .NET Framework 4.6.1+ and .NET Core 2.0+
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

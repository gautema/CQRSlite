# CQRSlite

[![Build](https://github.com/gautema/cqrslite/actions/workflows/ci.yml/badge.svg)](https://github.com/gautema/cqrslite/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/vpre/cqrslite.svg)](https://www.nuget.org/packages/cqrslite)

CQRSlite is a small framework for building applications with CQRS and event sourcing in C#. It gives you the building blocks (aggregates, a unit of work, repositories, a message router, snapshots and caching) and stays out of the way of everything else. Every part is behind an interface, so any of it can be replaced.

CQRSlite started as a CQRS sample project Greg Young and Gaute Magnussen made in 2010, which lives on at [gregoryyoung/m-r](https://github.com/gregoryyoung/m-r).

## Features

- Command sending, event publishing and queries, with handlers registered automatically
- A session that tracks aggregates as a unit of work
- A repository that loads aggregates from their events and saves new ones
- Optimistic concurrency checking
- Snapshots for aggregates with long histories
- Caching of aggregates between commands

## Installing

```
dotnet add package CqrsLite
```

CQRSlite targets .NET 10 and .NET Standard 2.0, so it also runs on .NET Framework 4.6.2 and later. Its only dependency is `Microsoft.Extensions.Caching.Memory`. You provide the event store for your database; see the [event store guide](docs/guides/event-store.md).

## A quick look

Business rules live in aggregates, which record events instead of changing state directly:

<!-- snippet: Sample/CQRSCode/WriteModel/Domain/InventoryItem.cs -->
```csharp
public class InventoryItem : AggregateRoot
{
    // ...

    private void Apply(ItemsCheckedInToInventory e)
    {
        _count += e.Count;
    }

    private void Apply(ItemsRemovedFromInventory e)
    {
        _count -= e.Count;
    }

    // ...

    public void Remove(int count)
    {
        if (count <= 0) throw new InvalidOperationException("cant remove negative count from inventory");
        if (count > _count) throw new InvalidOperationException($"cant remove {count} items, only {_count} in stock");
        ApplyChange(new ItemsRemovedFromInventory(Id, count));
    }

    // ...
}
```

Command handlers load an aggregate through the session, call it, and commit:

<!-- snippet: Sample/CQRSCode/WriteModel/Handlers/InventoryCommandHandlers.cs -->
```csharp
public async Task Handle(RemoveItemsFromInventory message, CancellationToken token)
{
    var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion, token);
    item.Remove(message.Count);
    await _session.Commit(token);
}
```

The rest of the application sends commands and queries through the router:

<!-- snippet: Sample/CQRSWeb/Controllers/HomeController.cs -->
```csharp
await _commandSender.Send(new RemoveItemsFromInventory(id, number, version), cancellationToken);
```

## Documentation

- [Concepts](docs/concepts.md): CQRS and event sourcing, and how CQRSlite's pieces fit together
- [Tutorial](docs/tutorial/1-overview.md): a walk through the [sample application](Sample), from commands to read models
- [Guides](docs/README.md#guides): aggregates, routing, event stores, snapshots and caching in depth
- [API reference](docs/reference/api.md)

## Further reading

These articles by others were written for older versions of CQRSlite, so details have changed, but the ideas still hold:

- [Real-World CQRS/ES with ASP.NET and Redis](https://web.archive.org/web/2019/https://www.exceptionnotfound.net/real-world-cqrs-es-with-asp-net-and-redis-part-1-overview/) by Matthew Jones, a five-part series (archived copy)
- [CQRS: A Cross Examination Of How It Works](https://www.codeproject.com/articles/991648/cqrs-a-cross-examination-of-how-it-works) on CodeProject

## Building

You need the .NET 10 SDK.

```
dotnet build
dotnet test
```

The docs are checked against the code: `python3 .github/scripts/check_docs.py` verifies that code snippets in the docs still match the files they come from, and that links between pages work. CI runs it on every push.

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

# Tutorial 4: The read model

The write model is good at enforcing rules, but it is no good for showing data. An `InventoryItem` doesn't even know its own name. The read side solves this by keeping its own data, shaped exactly the way the screens need it, and updating it from the events the write side publishes.

## Read models

A read model is data prepared ahead of time for one kind of query. The sample has two, one for each screen:

- the list of items on the front page, which needs each item's id and name
- the details page, which also needs the current count and version

Here is the details read model:

<!-- snippet: Sample/CQRSCode/ReadModel/Dtos/InventoryItemDetailsDto.cs -->
```csharp
public class InventoryItemDetailsDto
{
    public Guid Id;
    public string Name;
    public int CurrentCount;
    public int Version;
    // ...
}
```

The current count doesn't exist anywhere on the write side as a stored number; the aggregate works it out from its events each time it is loaded. The read model keeps it as a plain field so that showing it costs nothing. This is the core of CQRS: work done once when something changes, instead of every time someone looks.

The sample keeps its read models in an in-memory `InMemoryDatabase`. A real application would use a database table, a document store, a search index or a cache, whatever fits the query best. Different read models can use different storage.

## Event handlers

Event handlers keep the read models up to date. The details read model is maintained by `InventoryItemDetailView`, which handles every event that changes something it shows:

<!-- snippet: Sample/CQRSCode/ReadModel/Handlers/InventoryItemDetailView.cs -->
```csharp
public class InventoryItemDetailView : ICancellableEventHandler<InventoryItemCreated>,
    ICancellableEventHandler<InventoryItemDeactivated>,
    ICancellableEventHandler<InventoryItemRenamed>,
    ICancellableEventHandler<ItemsRemovedFromInventory>,
    ICancellableEventHandler<ItemsCheckedInToInventory>,
    ICancellableQueryHandler<GetInventoryItemDetails, InventoryItemDetailsDto>
{
    public Task Handle(InventoryItemCreated message, CancellationToken token)
    {
        InMemoryDatabase.Details.TryAdd(message.Id,
            new InventoryItemDetailsDto(message.Id, message.Name, 0, message.Version));
        return Task.CompletedTask;
    }

    // ...

    public Task Handle(ItemsCheckedInToInventory message, CancellationToken token)
    {
        var dto = GetDetailsItem(message.Id);
        dto.CurrentCount += message.Count;
        dto.Version = message.Version;
        return Task.CompletedTask;
    }

    public Task Handle(InventoryItemDeactivated message, CancellationToken token)
    {
        InMemoryDatabase.Details.TryRemove(message.Id, out _);
        return Task.CompletedTask;
    }

    // ...
}
```

Each `Handle` method makes a small, direct change to the read model. There is no business logic here: the decision was already made on the write side, and the event is a fact. If an event says five items were checked in, the read model adds five, no questions asked.

Every handler also copies the event's `Version` into the read model. That is what lets a screen send the right `ExpectedVersion` back with the next command, as [part 5](5-web-app.md) shows.

An event can have any number of handlers. `ItemsCheckedInToInventory` is handled by `InventoryItemDetailView` only, because the list doesn't show counts, while `InventoryItemRenamed` is handled by both views. Adding a new screen later means adding a new read model and its handlers, without touching the write side at all.

## Queries

A query is the read side's version of a command: a small class describing what you want to know. It implements `IQuery<TResult>`, where `TResult` is what you get back:

<!-- snippet: Sample/CQRSCode/ReadModel/Queries/GetInventoryItemDetails.cs -->
```csharp
public class GetInventoryItemDetails : IQuery<InventoryItemDetailsDto>
{
    public GetInventoryItemDetails(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }
}
```

The query handler is the last method of `InventoryItemDetailView`. It implements `ICancellableQueryHandler<GetInventoryItemDetails, InventoryItemDetailsDto>`:

<!-- snippet: Sample/CQRSCode/ReadModel/Handlers/InventoryItemDetailView.cs -->
```csharp
public Task<InventoryItemDetailsDto> Handle(GetInventoryItemDetails message, CancellationToken token = default)
{
    return Task.FromResult(GetDetailsItem(message.Id));
}
```

Because the read model already has the right shape, the query handler only has to look it up. Keeping the event handlers and the query handler for one read model in one class, as the sample does, keeps everything about that read model in one place. You can split them if you prefer; CQRSlite doesn't mind.

Each query type must have exactly one handler, like commands.

## Read models are disposable

The events are the source of truth, and a read model is only a view of them. If a read model has a bug, or you need a new one, you can delete it and rebuild it by replaying every stored event through its handlers. This is one of the big practical benefits of event sourcing, and a good reason to keep event handlers simple and free of side effects such as sending emails.

The sample doesn't show rebuilding, since its read models live only as long as the process. [Taking it to production](7-production.md) describes what you need for it.

## When the read model is updated

In the sample, the event store publishes events straight after storing them, and the router calls the event handlers before the command returns. So by the time the browser asks for the list again, the read model is already up to date.

That is a convenience of the sample, not something to rely on. In many real systems events are delivered through a queue, and the read model catches up a moment later. See [eventual consistency](../concepts.md#eventual-consistency).

Next: [the web app](5-web-app.md).

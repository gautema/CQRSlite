# Tutorial 2: The write model

The write model is where changes are decided. It is made of four kinds of things: commands that ask for a change, an aggregate that enforces the rules, events that record what happened, and command handlers that tie them together.

## Commands

A command is a plain class that implements `ICommand`. It carries everything needed to make the change, and nothing else. Here is the command for checking items in:

<!-- snippet: Sample/CQRSCode/WriteModel/Commands/CheckInItemsToInventory.cs -->
```csharp
public class CheckInItemsToInventory : ICommand
{
    public readonly int Count;

    public CheckInItemsToInventory(Guid id, int count, int originalVersion)
    {
        Id = id;
        Count = count;
        ExpectedVersion = originalVersion;
    }

    public Guid Id { get; set; }
    public int ExpectedVersion { get; set; }
}
```

`Id` says which inventory item the command is about. `ExpectedVersion` is the version of the item the user was looking at when they made the request. It is how the write side notices that someone else changed the item in the meantime, as explained in [Concepts](../concepts.md#versions-and-concurrency). `ICommand` itself requires neither property; they are a convention the sample uses for every command that changes an existing item.

Name commands in the imperative, after what the user wants to do. A command can be refused, so it is a request, not a fact.

## Events

An event records something that happened. It implements `IEvent`, which requires three properties:

<!-- snippet: Sample/CQRSCode/Events/ItemsCheckedInToInventory.cs -->
```csharp
public class ItemsCheckedInToInventory : IEvent
{
    public readonly int Count;

    public ItemsCheckedInToInventory(Guid id, int count)
    {
        Id = id;
        Count = count;
    }

    public Guid Id { get; set; }
    public int Version { get; set; }
    public DateTimeOffset TimeStamp { get; set; }
}
```

- `Id` is the id of the aggregate the event belongs to. If you leave it empty, CQRSlite fills it in from the aggregate when saving.
- `Version` is the event's position in the aggregate's history. CQRSlite sets it when saving.
- `TimeStamp` is set when saving too.

So the only thing you put in an event yourself is what happened: here, how many items were checked in. Name events in the past tense.

Events are stored forever, so think of them as a public contract. Adding a property later is fine; changing what an existing property means is not, because old events in the store will still have the old meaning.

## The aggregate

`InventoryItem` is the sample's only aggregate. This is where the business rules live:

<!-- snippet: Sample/CQRSCode/WriteModel/Domain/InventoryItem.cs -->
```csharp
public class InventoryItem : AggregateRoot
{
    // Only the state needed to enforce the rules below. Names etc. live in the read model.
    private bool _activated;
    private int _count;

    private void Apply(InventoryItemCreated e)
    {
        _activated = true;
    }

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

    public void CheckIn(int count)
    {
        if(count <= 0) throw new InvalidOperationException("must have a count greater than 0 to add to inventory");
        ApplyChange(new ItemsCheckedInToInventory(Id, count));
    }

    // ...

    private InventoryItem(){}
    public InventoryItem(Guid id, string name)
    {
        Id = id;
        ApplyChange(new InventoryItemCreated(id, name));
    }
}
```

There is a pattern in every public method: check the rules, then call `ApplyChange` with an event. The method never sets a field itself. `ApplyChange` does two things with the event: it applies it to the aggregate straight away, by calling the matching `Apply` method, and it adds it to the aggregate's list of uncommitted changes, which is what gets saved.

The `Apply` methods are the only place state changes. CQRSlite finds them by name and by the event type they take, whether they are public or private. They run in two situations:

- when a method records a new event through `ApplyChange`, as above
- when the aggregate is loaded, once for every event in its history, oldest first

Because the same code runs both times, the state you rebuild from history is always the state you had when you saved. This is why `Remove` can rely on `_count`: it was rebuilt from every check-in and removal before it.

Notice what the aggregate does *not* keep. It never stores the item's name, because no rule needs it: the name is only shown on screens, so it belongs to the read model. If an event has no `Apply` method, it is simply skipped, which is the case for `InventoryItemRenamed`. Keeping only the state your rules need keeps aggregates small and fast to load.

Two more details:

- The private parameterless constructor is what CQRSlite uses to create an empty `InventoryItem` before replaying its history. Every aggregate needs one; it can be private.
- The public constructor sets `Id` before recording the first event, because a new aggregate has no id until you give it one.

The [aggregates guide](../guides/aggregates.md) lists all the rules CQRSlite expects aggregates to follow.

## Command handlers

A command handler connects a command to the aggregate. The sample keeps all its command handlers in one class:

<!-- snippet: Sample/CQRSCode/WriteModel/Handlers/InventoryCommandHandlers.cs -->
```csharp
public class InventoryCommandHandlers : ICommandHandler<CreateInventoryItem>,
    ICancellableCommandHandler<DeactivateInventoryItem>,
    ICancellableCommandHandler<RemoveItemsFromInventory>,
    ICancellableCommandHandler<CheckInItemsToInventory>,
    ICancellableCommandHandler<RenameInventoryItem>
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

    // ...

    public async Task Handle(CheckInItemsToInventory message, CancellationToken token)
    {
        var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion, token);
        item.CheckIn(message.Count);
        await _session.Commit(token);
    }

    // ...
}
```

Every handler follows the same three steps: get the aggregate, call one method on it, commit.

- `ICommandHandler<T>` gives you `Handle(T message)`. `ICancellableCommandHandler<T>` adds a `CancellationToken`, which the router passes along from whoever sent the command (in the web app, the request's token). Prefer the cancellable one.
- A new aggregate is handed to the session with `Add`. An existing one is loaded with `Get`.
- Passing `ExpectedVersion` to `Get` makes the session check it straight away: if the item is no longer at that version, `Get` throws a `ConcurrencyException` and nothing changes.
- `Commit` saves every aggregate the session has handed out. Until then, nothing is stored.

Keep handlers this thin. If a handler starts to contain `if` statements about the business, those rules probably belong in the aggregate, where they can't be bypassed and are easy to test.

## The session

The session (`ISession`, implemented by `Session`) is the unit of work. It remembers which aggregates it has handed out and saves them all on `Commit`. If you `Get` the same aggregate twice in one session, you get the same object back, so two parts of one command can't work on different copies.

One session per command, or per web request, is the right scope. In the web app it is registered as a scoped service for that reason.

A session can track more than one aggregate, but `Commit` saves them one after the other, not in a single transaction. If the second save fails, the first is already stored. That is the main reason for the usual advice: change one aggregate per command. When a business process spans several aggregates, let an event from the first trigger a command for the next.

Next: [the event store](3-event-store.md).

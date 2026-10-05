# Aggregates

Read this when you write your own aggregates. It lists every rule CQRSlite expects them to follow, and the mistakes that are easy to make. [The write model part of the tutorial](../tutorial/2-write-model.md) explains the ideas with the sample's `InventoryItem`.

## The shape of an aggregate

An aggregate inherits `AggregateRoot`. Its public methods check the business rules and then record what happened with `ApplyChange`. Its `Apply` methods are the only place its state changes:

<!-- snippet: Sample/CQRSCode/WriteModel/Domain/InventoryItem.cs -->
```csharp
public class InventoryItem : AggregateRoot
{
    // Only the state needed to enforce the rules below. Names etc. live in the read model.
    private bool _activated;
    private int _count;
    // ...
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
    private InventoryItem(){}
    public InventoryItem(Guid id, string name)
    {
        Id = id;
        ApplyChange(new InventoryItemCreated(id, name));
    }
}
```

## Constructors

- **A parameterless constructor is required.** The repository uses it to create an empty aggregate before replaying its history. It can, and usually should, be private. Without one, loading throws `MissingParameterLessConstructorException`.
- **The creating constructor sets `Id` first**, then records the creation event. A new aggregate has no id until you give it one, and its events take their id from it.
- The parameterless constructor should not record events or set state. Everything comes from the history.

## Recording changes

`ApplyChange(event)` does two things, in this order:

1. It applies the event to the aggregate straight away, through `ApplyEvent`, which calls the matching `Apply` method. The state is updated by the time `ApplyChange` returns.
2. It adds the event to the aggregate's uncommitted changes. Those are what the repository saves.

`Version` does not change here. It goes up when the aggregate is saved, by the number of events saved.

Check every rule *before* calling `ApplyChange`. A method that throws after recording an event leaves the aggregate changed. The session won't save it unless you commit, but it's still a confusing state to be in.

## Apply methods

`AggregateRoot.ApplyEvent` finds the method to call by convention:

- It is named `Apply` and takes exactly one parameter: the event.
- Any visibility works. Make them private, so nothing outside the aggregate can change its state without an event.
- A method whose parameter is exactly the event's type is preferred. Otherwise, one whose parameter type the event can be assigned to is used, such as an interface the event implements.
- The aggregate's own class is searched first, then its base classes.
- **If there is no matching method, the event is skipped.** That's intended: keep only the state your rules need, and leave everything else to the read model.

The lookup uses reflection once per aggregate type, event type and method name. After that it calls a compiled delegate, so the convention costs almost nothing.

If you prefer to dispatch yourself, override `ApplyEvent`:

```csharp
public class InventoryItemWithSwitch : AggregateRoot
{
    private int _count;

    private InventoryItemWithSwitch() { }

    protected override void ApplyEvent(IEvent @event)
    {
        switch (@event)
        {
            case ItemsCheckedInToInventory e:
                _count += e.Count;
                break;
            case ItemsRemovedFromInventory e:
                _count -= e.Count;
                break;
            default:
                base.ApplyEvent(@event); // Fall back to Apply methods
                break;
        }
    }
}
```

An `Apply` method must only change state. It runs again every time the aggregate is loaded, so it must not check rules, throw for ordinary events, call services or record more events.

## Events

Events implement `IEvent`, which has three properties the framework manages:

- **`Id`** is the id of the aggregate the event belongs to. If you leave it empty, it is filled in from the aggregate when saving. If you set it to anything else, saving throws `EventIdIncorrectException`. If both the event's and the aggregate's id are empty, saving throws `AggregateOrEventMissingIdException`.
- **`Version`** is set when saving: the aggregate's version plus the event's position among the new events.
- **`TimeStamp`** is set to the current UTC time when saving. Anything you put there is overwritten.

Designing them well matters more than anywhere else, because they are stored forever:

- **Name them in the past tense, after what happened in the business**: `ItemsCheckedInToInventory`, not `InventoryCountChanged` or `UpdateItem`. An event per meaningful change beats one generic "state changed" event, both for the read model and for anyone reading the history later.
- **Make them self-contained.** Include what the event handlers need, so they don't have to look anything up to know what happened.
- **Keep them as plain data** that your serializer can round-trip. See [Storing events](event-store.md#storing-events) for how to change them over time.

## Loading

The repository loads an aggregate by creating it with the parameterless constructor and calling `LoadFromHistory` with its events. Each event's `Apply` method runs, and `Version` ends up at the last event's version.

`LoadFromHistory` checks that the history is consistent. It throws `EventsOutOfOrderException` if a version doesn't follow on from the previous one, and `EventIdIncorrectException` if an event belongs to another aggregate. Either one means the event store returned the wrong events.

`LoadFromHistory`, `GetUncommittedChanges` and `FlushUncommittedChanges` are public so repositories and tests can use them. Your domain code shouldn't call them. In particular, `FlushUncommittedChanges` assigns versions and clears the changes. Calling it outside a repository loses events.

## Boundaries

**Change one aggregate per command.** `Session.Commit` saves each changed aggregate in turn, and it isn't a transaction. If the second save fails, for example with a `ConcurrencyException`, the first one stays saved.

When a change has to reach several aggregates, let the first aggregate record its event. Then react to that event with an event handler that sends a command to the next one: a *process manager*. Each step is consistent on its own, and the whole flow completes eventually.

**Keep aggregates small.** Every command loads the whole history of the aggregate it changes. An aggregate whose history keeps growing, like a "customer" that records every order, gets slower to load forever. Model the long-lived thing as several aggregates, or add [snapshots](snapshots.md) if it really has to be one.

**Don't query aggregates for screens.** Aggregates are for enforcing rules. Lists, details and searches come from the [read model](../tutorial/4-read-model.md).

## Common mistakes

- **Changing a field outside an `Apply` method.** The change is lost the next time the aggregate is loaded, because only events are stored.
- **Checking rules inside `Apply` methods.** They run for old events too. A rule you add later would make historical events throw, and the aggregate would never load again.
- **Forgetting the parameterless constructor**, or giving it logic.
- **Not committing.** Changes recorded with `ApplyChange` are only saved by `ISession.Commit` (or `IRepository.Save`).
- **Recording an event, then throwing.** Check every rule before the first `ApplyChange`.
- **Ignoring `ConcurrencyException`.** It means someone else changed the aggregate since it was read. Report it to the user, or reload and try again if the command still makes sense. See [errors in handlers and routing](handlers-and-routing.md#errors).

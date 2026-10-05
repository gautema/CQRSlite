# Tutorial 6: Testing

Event sourcing makes the write side unusually easy to test. An aggregate's state comes entirely from its events, and its behaviour is to produce new events. So almost every test has the same shape:

- **Given** these events have happened,
- **when** this command is handled,
- **then** these new events should be produced.

You never need a database, and you never test private state. You test what the aggregate *does*, which is what you care about. The sample's tests are in [`Sample/CQRSTest`](../../Sample/CQRSTest).

## Specification tests

The repository includes a small helper for this style of test, `Specification<TAggregate, THandler, TCommand>`, in [`Framework/CQRSlite.Tests.Extensions`](../../Framework/CQRSlite.Tests.Extensions/TestHelpers). It is not published as a package, so copy its two files (`Specification.cs` and `ThenAttribute.cs`) into your own test project. They depend on CQRSlite and xUnit.

Here is a test for checking items in:

<!-- snippet: Sample/CQRSTest/WriteModel/WhenItemCheckedIn.cs -->
```csharp
public class When_item_checked_in : Specification<InventoryItem, InventoryCommandHandlers, CheckInItemsToInventory>
{
    private Guid _guid;

    protected override InventoryCommandHandlers BuildHandler()
    {
        return new InventoryCommandHandlers(Session);
    }

    protected override IEnumerable<IEvent> Given()
    {
        _guid = Guid.NewGuid();
        return new List<IEvent>
        {
            new InventoryItemCreated(_guid, "Jadda") {Version = 1},
            new ItemsCheckedInToInventory(_guid, 2) {Version = 2}
        };
    }

    protected override CheckInItemsToInventory When()
    {
        return new CheckInItemsToInventory(_guid, 2, 2);
    }

    [Then]
    public void Should_create_one_event()
    {
        Assert.Single(PublishedEvents);
    }

    [Then]
    public void Should_create_correct_event()
    {
        Assert.IsType<ItemsCheckedInToInventory>(PublishedEvents.First());
    }

    [Then]
    public void Should_save_have_correct_number_of_items()
    {
        Assert.Equal(2, ((ItemsCheckedInToInventory)PublishedEvents.First()).Count);
    }
}
```

The test class describes one scenario, and each `[Then]` method checks one thing about the outcome. Here is what the helper does:

- `Given()` returns the history the aggregate starts with. Each event needs its `Version` set, in order from 1, because that is how they would have been stored.
- `BuildHandler()` creates the command handler. The helper provides a `Session` backed by an in-memory event store pre-loaded with the given events; pass it in.
- `When()` returns the command to run.
- The helper runs the command in the constructor, before any `[Then]` method, and waits for the handler to finish.
- `PublishedEvents` holds the new events the command produced. `EventDescriptors` holds everything in the event store afterwards, given events included. `Aggregate` is the aggregate the given events belong to. It is loaded through the same session as the handler uses, so afterwards it is the very object the command changed. It is null if `Given()` returns no events.

`[Then]` works like xUnit's `[Fact]`; it exists so the tests read as a specification.

Note that the test never checks the aggregate's fields. It checks the events. If you later change how `InventoryItem` stores its state, these tests keep passing as long as the behaviour is the same.

## Testing rules

A specification test fails in its constructor if the handler throws, so it is not the right tool for checking that a command is refused. For that, test the aggregate directly. It is a plain object, so this is short:

<!-- snippet: Sample/CQRSTest/WriteModel/WhenRemovingMoreThanInStock.cs -->
```csharp
public class When_removing_more_items_than_in_stock
{
    [Fact]
    public void Should_refuse()
    {
        var item = new InventoryItem(Guid.NewGuid(), "Widget");
        item.CheckIn(2);

        Assert.Throws<InvalidOperationException>(() => item.Remove(3));
    }

    // ...
}
```

The "given" part here is calling the aggregate's own methods, which record events just as the history would. `GetUncommittedChanges()` returns the events recorded so far, if you want to assert on them.

## What to test where

- **Aggregates** carry your business rules, so they deserve the most tests: one scenario per rule, both the allowed and the refused case.
- **Command handlers** are thin. A specification test per command is usually enough to show the handler loads the right aggregate and calls the right method.
- **Read models** are simple too, but they are easy to test the same way: given these events, the read model should contain this. Call the event handlers' `Handle` methods with events and check the result.
- **The wiring** in `Program.cs` is best covered by a few end-to-end tests with ASP.NET Core's `WebApplicationFactory`, since a missing registration only shows up when a message is sent.

Next: [taking it to production](7-production.md).

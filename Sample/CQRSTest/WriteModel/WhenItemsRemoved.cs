using CQRSCode.Events;
using CQRSCode.WriteModel.Commands;
using CQRSCode.WriteModel.Domain;
using CQRSCode.WriteModel.Handlers;
using CQRSlite.Events;
using CQRSlite.Tests.Extensions.TestHelpers;
using Xunit;

namespace CQRSTest.WriteModel;

public class When_items_removed : Specification<InventoryItem, InventoryCommandHandlers, RemoveItemsFromInventory>
{
    private Guid _id;

    protected override InventoryCommandHandlers BuildHandler()
    {
        return new InventoryCommandHandlers(Session);
    }

    protected override IEnumerable<IEvent> Given()
    {
        _id = Guid.NewGuid();
        return new List<IEvent>
        {
            new InventoryItemCreated(_id, "Widget") { Version = 1 },
            new ItemsCheckedInToInventory(_id, 5) { Version = 2 }
        };
    }

    protected override RemoveItemsFromInventory When()
    {
        return new RemoveItemsFromInventory(_id, 2, 2);
    }

    [Then]
    public void Should_record_the_removal()
    {
        var removed = Assert.IsType<ItemsRemovedFromInventory>(Assert.Single(PublishedEvents));
        Assert.Equal(2, removed.Count);
        Assert.Equal(3, removed.Version);
    }

    [Then]
    public void Should_have_saved_the_aggregate()
    {
        Assert.NotNull(Aggregate);
        Assert.Equal(3, Aggregate.Version);
    }
}

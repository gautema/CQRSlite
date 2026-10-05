using CQRSCode.WriteModel.Domain;
using Xunit;

namespace CQRSTest.WriteModel;

// Aggregates are plain objects, so business rules can be tested without any infrastructure
public class When_removing_more_items_than_in_stock
{
    [Fact]
    public void Should_refuse()
    {
        var item = new InventoryItem(Guid.NewGuid(), "Widget");
        item.CheckIn(2);

        Assert.Throws<InvalidOperationException>(() => item.Remove(3));
    }

    [Fact]
    public void Should_allow_removing_what_is_in_stock()
    {
        var item = new InventoryItem(Guid.NewGuid(), "Widget");
        item.CheckIn(2);

        item.Remove(2);

        Assert.Equal(3, item.GetUncommittedChanges().Length);
    }
}

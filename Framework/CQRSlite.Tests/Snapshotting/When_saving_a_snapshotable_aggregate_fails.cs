using CQRSlite.Snapshotting;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Snapshotting;

public class When_saving_a_snapshotable_aggregate_fails
{
    [Fact]
    public async Task Should_not_save_snapshot()
    {
        var snapshotStore = new TestInMemorySnapshotStore();
        var repository = new SnapshotRepository(snapshotStore, new DefaultSnapshotStrategy(1),
            new TestRepository { Throw = true }, new TestInMemoryEventStore());
        var aggregate = new TestSnapshotAggregate(Guid.NewGuid());
        aggregate.DoSomething();

        await Assert.ThrowsAnyAsync<Exception>(() => repository.Save(aggregate));

        Assert.Null(await snapshotStore.Get(aggregate.Id));
    }
}

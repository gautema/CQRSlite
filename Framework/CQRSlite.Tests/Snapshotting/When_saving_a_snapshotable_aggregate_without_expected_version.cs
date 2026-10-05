using CQRSlite.Domain;
using CQRSlite.Snapshotting;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Snapshotting;

public class When_saving_a_snapshotable_aggregate_without_expected_version
{
    [Fact]
    public async Task Should_snapshot_with_version_after_events_from_others_are_loaded()
    {
        var eventStore = new TestInMemoryEventStore();
        var snapshotStore = new TestInMemorySnapshotStore();
        var repository = new SnapshotRepository(snapshotStore, new DefaultSnapshotStrategy(2), new Repository(eventStore), eventStore);
        var id = Guid.NewGuid();

        var created = new TestSnapshotAggregate(id);
        created.DoSomething();
        await repository.Save(created);

        var stale = await new Repository(eventStore).Get<TestSnapshotAggregate>(id);
        var other = await new Repository(eventStore).Get<TestSnapshotAggregate>(id);
        other.DoSomething();
        other.DoSomething();
        await new Repository(eventStore).Save(other);

        // Without expected version the save loads the other writer's events before its own
        stale.DoSomething();
        await repository.Save(stale);

        var snapshot = await snapshotStore.Get(id);
        Assert.NotNull(snapshot);
        Assert.Equal(eventStore.Events.Count, snapshot.Version);
    }
}

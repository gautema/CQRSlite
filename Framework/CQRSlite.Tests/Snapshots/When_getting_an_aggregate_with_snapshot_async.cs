using CQRSlite.Domain;
using CQRSlite.Snapshots;
using CQRSlite.Tests.Substitutes;
using System;
using Xunit;

namespace CQRSlite.Tests.Snapshots
{
    public class When_getting_an_aggregate_with_snapshot_async
    {
        private TestSnapshotAggregate _aggregate;

        public When_getting_an_aggregate_with_snapshot_async()
        {
            var eventStore = new TestInMemoryEventStore();
            var snapshotStore = new TestSnapshotStore();
            var snapshotStrategy = new DefaultSnapshotStrategy();
            var snapshotRepository = new SnapshotRepository(snapshotStore, snapshotStrategy, new Repository(eventStore), eventStore);
            var session = new Session(snapshotRepository);

            _aggregate = session.GetAsync<TestSnapshotAggregate>(Guid.NewGuid()).Result;
        }

        [Fact]
        public void Should_restore()
        {
            Assert.True(_aggregate.Restored);
        }
    }
}

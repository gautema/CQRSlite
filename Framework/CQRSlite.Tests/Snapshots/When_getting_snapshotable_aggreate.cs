using System;
using CQRSlite.Domain;
using CQRSlite.Snapshots;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Snapshots
{
    public class When_getting_snapshotable_aggreate
    {
        private TestSnapshotStore _snapshotStore;
        private TestSnapshotAggregate _aggregate;

        public When_getting_snapshotable_aggreate()
        {
            var eventStore = new TestInMemoryEventStore();
            _snapshotStore = new TestSnapshotStore();
            var snapshotStrategy = new DefaultSnapshotStrategy();
            var repository = new SnapshotRepository(_snapshotStore, snapshotStrategy, new Repository(eventStore), eventStore);
            var session = new Session(repository);

            _aggregate = session.Get<TestSnapshotAggregate>(Guid.NewGuid());
        }

        [Fact]
        public void Should_ask_for_snapshot()
        {
            Assert.True(_snapshotStore.VerifyGet);
        }

        [Fact]
        public void Should_run_restore_method()
        {
            Assert.True(_aggregate.Restored);
        }
    }
}

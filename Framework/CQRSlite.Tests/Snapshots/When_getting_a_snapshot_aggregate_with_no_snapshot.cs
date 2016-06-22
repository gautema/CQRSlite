using System;
using CQRSlite.Domain;
using CQRSlite.Snapshots;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Snapshots
{
    public class When_getting_a_snapshot_aggregate_with_no_snapshot
    {
        private TestSnapshotAggregate _aggregate;

        public When_getting_a_snapshot_aggregate_with_no_snapshot()
        {
            var eventStore = new TestEventStore();
            var snapshotStore = new NullSnapshotStore();
            var snapshotStrategy = new DefaultSnapshotStrategy();
            var repository = new SnapshotRepository(snapshotStore, snapshotStrategy, new Repository(eventStore), eventStore);
            var session = new Session(repository);
            _aggregate = session.Get<TestSnapshotAggregate>(Guid.NewGuid());
        }

	    private class NullSnapshotStore : ISnapshotStore
	    {
	        public Snapshot Get(Guid id)
	        {
	            return null;
	        }
            public void Save(Snapshot snapshot){}
	    }

	    [Fact]
        public void Should_load_events()
        {
            Assert.True(_aggregate.Loaded);
        }

        [Fact]
        public void Should_not_load_snapshot()
        {
            Assert.False(_aggregate.Restored);
        }
    }
}
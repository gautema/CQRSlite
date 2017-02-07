using CQRSlite.Domain;
using CQRSlite.Snapshots;
using CQRSlite.Tests.Substitutes;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CQRSlite.Tests.Snapshots
{
    public class When_getting_a_snapshot_aggregate_with_no_snapshot_async
    {
        private TestSnapshotAggregate _aggregate;

        public When_getting_a_snapshot_aggregate_with_no_snapshot_async()
        {
            var eventStore = new TestEventStore();
            var snapshotStore = new NullSnapshotStore();
            var snapshotStrategy = new DefaultSnapshotStrategy();
            var repository = new SnapshotRepository(snapshotStore, snapshotStrategy, new Repository(eventStore), eventStore);
            var session = new Session(repository);
            _aggregate = session.GetAsync<TestSnapshotAggregate>(Guid.NewGuid()).Result;
        }

        private class NullSnapshotStore : ISnapshotStore
        {
            public Snapshot Get(Guid id)
            {
                return null;
            }

            public Task<Snapshot> GetAsync(Guid id)
            {
                return Task.FromResult<Snapshot>(null);
            }

            public void Save(Snapshot snapshot) { }

            public Task SaveAsync(Snapshot snapshot)
            {
                return Task.FromResult(0);
            }
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

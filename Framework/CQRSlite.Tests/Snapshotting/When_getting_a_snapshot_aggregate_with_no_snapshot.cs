using System;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Domain;
using CQRSlite.Snapshotting;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Snapshotting
{
    public class When_getting_a_snapshot_aggregate_with_no_snapshot
    {
        private readonly TestSnapshotAggregate _aggregate;
        private readonly CancellationToken _token;
        private readonly TestEventStore _eventStore;

        public When_getting_a_snapshot_aggregate_with_no_snapshot()
        {
            _eventStore = new TestEventStore();
            var snapshotStore = new NullSnapshotStore();
            var snapshotStrategy = new DefaultSnapshotStrategy();
            var repository = new SnapshotRepository(snapshotStore, snapshotStrategy, new Repository(_eventStore), _eventStore);
            var session = new Session(repository);
            _token = new CancellationToken();
            _aggregate = session.Get<TestSnapshotAggregate>(Guid.NewGuid(), cancellationToken: _token).Result;
        }

	    private class NullSnapshotStore : ISnapshotStore
	    {
	        public Task<Snapshot> Get(Guid id, CancellationToken cancellationToken = default(CancellationToken))
	        {
	            return Task.FromResult<Snapshot>(null);
	        }
            public Task Save(Snapshot snapshot, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.CompletedTask;
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

        [Fact]
        public void Should_forward_cancellation_token()
        {
            Assert.Equal(_token, _eventStore.Token);
        }
    }
}
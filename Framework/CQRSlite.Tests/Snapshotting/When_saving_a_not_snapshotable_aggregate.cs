using System;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Domain;
using CQRSlite.Snapshotting;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Snapshotting
{
    public class When_saving_a_not_snapshotable_aggregate
    {
        private readonly TestSnapshotStore _snapshotStore;
        private readonly TestInMemoryEventStore _eventStore;
        private readonly CancellationToken _token;

        public When_saving_a_not_snapshotable_aggregate()
        {
            _eventStore = new TestInMemoryEventStore();
            _snapshotStore = new TestSnapshotStore();
            var snapshotStrategy = new DefaultSnapshotStrategy();
            var repository = new SnapshotRepository(_snapshotStore, snapshotStrategy, new Repository(_eventStore), _eventStore);
            var session = new Session(repository);
            var aggregate = new TestAggregate(Guid.NewGuid());
            _token = new CancellationToken();

            for (var i = 0; i < 200; i++)
            {
                aggregate.DoSomething();
            }
            Task.Run(async () =>
            {
                await session.Add(aggregate, _token);
                await session.Commit(_token);
            }).Wait();
        }

        [Fact]
        public void Should_not_save_snapshot()
        {
            Assert.False(_snapshotStore.VerifySave);
        }

        [Fact]
        public void Should_save_events()
        {
            Assert.Equal(201, _eventStore.Events.Count);
        }

        [Fact]
        public void Should_forward_cancellation_token()
        {
            Assert.Equal(_token, _eventStore.Token);
        }
    }
}

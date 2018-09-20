using System;
using CQRSlite.Domain;
using CQRSlite.Snapshotting;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Snapshotting
{
    public class When_getting_not_snapshotable_aggregate
    {
        private readonly TestSnapshotStore _snapshotStore;
        private readonly TestAggregate _aggregate;

        public When_getting_not_snapshotable_aggregate()
        {
            var eventStore = new TestEventStore();
            _snapshotStore = new TestSnapshotStore();
            var snapshotStrategy = new DefaultSnapshotStrategy();
            var repository = new SnapshotRepository(_snapshotStore, snapshotStrategy, new Repository(eventStore), eventStore);
            var session = new Session(repository);

            _aggregate = session.Get<TestAggregate>(Guid.NewGuid()).Result;
        }

        [Fact]
        public void Should_not_ask_for_snapshot()
        {
            Assert.False(_snapshotStore.VerifyGet);
        }

        [Fact]
        public void Should_restore_from_events()
        {
            Assert.Equal(3, _aggregate.Version);
        }
    }
}

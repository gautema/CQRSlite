using System;
using CQRSlite.Domain;
using CQRSlite.Eventing;
using CQRSlite.Tests.TestSubstitutes;
using Xunit;

namespace CQRSlite.Tests.DomainTests
{
    public class WhenGettingASnapshotAggregateWithNoSnapshot
    {
        private TestSnapshotAggreagate _aggregate;

        public WhenGettingASnapshotAggregateWithNoSnapshot()
        {
            var eventStore = new TestEventStore();
            var eventPublisher = new TestEventPublisher();
            var snapshotStore = new NullSnapshotStore();
            var rep = new Repository<TestSnapshotAggreagate>(eventStore, snapshotStore, eventPublisher);
            _aggregate = rep.Get(Guid.NewGuid());
        }

        [Fact]
        public void ShouldLoadEvents()
        {
            Assert.True(_aggregate.Loaded);
        }

        [Fact]
        public void ShouldNotLoadSnapshot()
        {
            Assert.False(_aggregate.Restored);
        }
    }
}
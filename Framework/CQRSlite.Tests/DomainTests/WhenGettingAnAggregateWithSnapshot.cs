using System;
using CQRSlite.Domain;
using CQRSlite.Tests.TestSubstitutes;
using Xunit;

namespace CQRSlite.Tests.DomainTests
{
    public class WhenGettingAnAggregateWithSnapshot
    {
        private TestSnapshotAggreagate _aggregate;

        public WhenGettingAnAggregateWithSnapshot()
        {
            var eventStore = new TestEventStore();
            var eventPublisher = new TestEventPublisher();
            var snapshotStore = new TestSnapshotStore();
            var rep = new Repository<TestSnapshotAggreagate>(eventStore, snapshotStore, eventPublisher);
            _aggregate = rep.Get(Guid.NewGuid());
        }

        [Fact]
        public void ShouldRestore()
        {
            Assert.True(_aggregate.Restored);
        }
    }
}

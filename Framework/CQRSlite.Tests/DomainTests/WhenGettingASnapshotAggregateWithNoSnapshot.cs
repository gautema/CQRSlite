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
            var snapshotStore = new NullSnapshotStore();
            var rep = new Repository<TestSnapshotAggreagate>(eventStore, snapshotStore);
            _aggregate = rep.GetById(Guid.NewGuid());
        }

        [Fact]
        public void ShouldLoadEvents()
        {
            Assert.Equal(3, _aggregate.Version);
        }
    }
}
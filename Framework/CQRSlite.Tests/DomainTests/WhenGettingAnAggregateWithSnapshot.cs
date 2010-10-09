using System;
using CQRSlite.Domain;
using CQRSlite.Tests.TestSubstitutes;
using Xunit;

namespace CQRSlite.Tests.DomainTests
{
    public class WhenGettingAnAggregateWithSnapshot
    {
        private TestSnapshotStore _snapshotStore;
        private TestAggregate _aggregate;

        public WhenGettingAnAggregateWithSnapshot()
        {
            var eventStore = new TestEventStore();
            _snapshotStore = new TestSnapshotStore();
            var rep = new Repository<TestAggregate>(eventStore, _snapshotStore);
            _aggregate = rep.GetById(Guid.NewGuid());
        }

        [Fact]
        public void ShouldLoadVersion()
        {
            
        }
    }
}

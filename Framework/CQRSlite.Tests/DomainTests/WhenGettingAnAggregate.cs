using System;
using CQRSlite.Domain;
using CQRSlite.Tests.TestSubstitutes;
using Xunit;

namespace CQRSlite.Tests.DomainTests
{
    public class WhenGettingAnAggregate
    {
        private TestAggregate _aggregate;
        private TestSnapshotStore _snapshotStore;

        public WhenGettingAnAggregate()
        {
            var eventStore = new TestEventStore();
            _snapshotStore = new TestSnapshotStore();
            var rep = new Repository<TestAggregate>(eventStore, _snapshotStore);
            _aggregate = rep.GetById(Guid.NewGuid());
        }

        [Fact]
        public void ShouldGetAggreagateFromEventStore()
        {
            Assert.NotNull(_aggregate);
        }

        [Fact]
        public void ShouldApplyEvents()
        {
            Assert.Equal(2,_aggregate.I);
        }

        [Fact]
        public void ShouldCheckSnapshotStore()
        {
            Assert.True(_snapshotStore.VerifyGet);
        }
    }
}
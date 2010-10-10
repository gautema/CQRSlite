using System;
using CQRSlite.Domain;
using CQRSlite.Eventing;
using CQRSlite.Tests.TestSubstitutes;
using Xunit;

namespace CQRSlite.Tests.DomainTests
{
    public class WhenGettingAnAggregate
    {
        private readonly TestAggregate _aggregate;

        public WhenGettingAnAggregate()
        {
            var eventStore = new TestEventStore();
            var snapshotStore = new NullSnapshotStore();
            var rep = new Repository<TestAggregate>(eventStore, snapshotStore);
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
    }
}
using System;
using CQRSlite.Domain;
using CQRSlite.Eventing;
using CQRSlite.Tests.TestSubstitutes;
using Xunit;

namespace CQRSlite.Tests.DomainTests
{
    public class WhenGettingAnAggregate
    {

        private Repository<TestAggregate> _rep;

        public WhenGettingAnAggregate()
        {
            var eventStore = new TestEventStore();
            var testEventPublisher = new TestEventPublisher();
            var snapshotStore = new NullSnapshotStore();
            _rep = new Repository<TestAggregate>(eventStore, snapshotStore, testEventPublisher);

        }

        [Fact]
        public void ShouldGetAggreagateFromEventStore()
        {
            var aggregate = _rep.Get(Guid.NewGuid());
            Assert.NotNull(aggregate);
        }

        [Fact]
        public void ShouldApplyEvents()
        {
            var aggregate = _rep.Get(Guid.NewGuid());
            Assert.Equal(2,aggregate.I);
        }

        [Fact]
        public void ShouldFailIfAggregateNotExists()
        {
            Assert.Throws<AggregateNotFoundException>(() => { _rep.Get(Guid.Empty); });
        }
    }
}
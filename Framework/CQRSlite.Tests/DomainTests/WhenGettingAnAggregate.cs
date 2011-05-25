using System;
using CQRSlite.Domain;
using CQRSlite.Eventing;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.DomainTests
{
	[TestFixture]
    public class WhenGettingAnAggregate
    {

        private Repository<TestAggregate> _rep;
		
		[SetUp]
        public void Setup()
        {
            var eventStore = new TestEventStore();
            var testEventPublisher = new TestEventPublisher();
            var snapshotStore = new NullSnapshotStore();
            _rep = new Repository<TestAggregate>(eventStore, snapshotStore, testEventPublisher);

        }

        [Test]
        public void ShouldGetAggreagateFromEventStore()
        {
            var aggregate = _rep.Get(Guid.NewGuid());
            Assert.NotNull(aggregate);
        }

        [Test]
        public void ShouldApplyEvents()
        {
            var aggregate = _rep.Get(Guid.NewGuid());
            Assert.AreEqual(2,aggregate.I);
        }

        [Test]
        public void ShouldFailIfAggregateNotExists()
        {
            Assert.Throws<AggregateNotFoundException>(() => { _rep.Get(Guid.Empty); });
        }
    }
}
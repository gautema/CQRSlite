using System;
using CQRSlite.Domain;
using CQRSlite.Eventing;
using CQRSlite.Infrastructure;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.DomainTests
{
	[TestFixture]
    public class When_getting_an_aggregate
    {

        private Repository<TestAggregate> _rep;
		
		[SetUp]
        public void Setup()
        {
            var eventStore = new TestEventStore();
            var testEventPublisher = new TestEventPublisher();
            var snapshotStore = new NullSnapshotStore();
		    var snapshotStrategy = new DefaultSnapshotStrategy();
            _rep = new Repository<TestAggregate>(eventStore, snapshotStore, testEventPublisher, snapshotStrategy);
        }

        [Test]
        public void Should_get_aggregate_from_eventstore()
        {
            var aggregate = _rep.Get(Guid.NewGuid());
            Assert.NotNull(aggregate);
        }

        [Test]
        public void Should_applye_events()
        {
            var aggregate = _rep.Get(Guid.NewGuid());
            Assert.AreEqual(2,aggregate.I);
        }

        [Test]
        public void Should_fail_if_aggregate_do_not_exist()
        {
            Assert.Throws<AggregateNotFoundException>(() => { _rep.Get(Guid.Empty); });
        }
    }
}
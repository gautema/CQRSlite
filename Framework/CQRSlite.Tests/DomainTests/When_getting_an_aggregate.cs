using System;
using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using CQRSlite.Eventing;
using CQRSlite.Infrastructure;
using CQRSlite.Snapshotting;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.DomainTests
{
	[TestFixture]
    public class When_getting_an_aggregate
    {
	    private ISession _session;

	    [SetUp]
        public void Setup()
        {
            var eventStore = new TestEventStore();
            var testEventPublisher = new TestEventPublisher();
            var snapshotStore = new NullSnapshotStore();
            var snapshotStrategy = new DefaultSnapshotStrategy();
            _session = new Session(new Repository(eventStore, testEventPublisher, snapshotStore, snapshotStrategy));
        }

        [Test]
        public void Should_get_aggregate_from_eventstore()
        {
            var aggregate = _session.Get<TestAggregate>(Guid.NewGuid());
            Assert.NotNull(aggregate);
        }

        [Test]
        public void Should_apply_events()
        {
            var aggregate = _session.Get<TestAggregate>(Guid.NewGuid());
            Assert.AreEqual(2,aggregate.I);
        }

        [Test]
        public void Should_fail_if_aggregate_do_not_exist()
        {
            Assert.Throws<AggregateNotFoundException>(() => _session.Get<TestAggregate>(Guid.Empty));
        }

        [Test]
	    public void Should_track_changes()
	    {
            var agg = new TestAggregate(Guid.NewGuid());
            _session.Add(agg);
            var aggregate = _session.Get<TestAggregate>(agg.Id);
            Assert.AreEqual(agg,aggregate);
	    }

        [Test]
        public void Should_get_from_session_if_tracked()
        {
            var id = Guid.NewGuid();
            var aggregate = _session.Get<TestAggregate>(id);
            var aggregate2 = _session.Get<TestAggregate>(id);

            Assert.AreEqual(aggregate, aggregate2);
        }

        [Test]
        public void Should_throw_concurrency_exception_if_tracked()
        {
            var id = Guid.NewGuid();
            _session.Get<TestAggregate>(id);

            Assert.Throws<ConcurrencyException>(() => _session.Get<TestAggregate>(id, 100));
        }

        [Test]
        public void Should_get_correct_version()
        {
            var id = Guid.NewGuid();
            var aggregate = _session.Get<TestAggregate>(id);

            Assert.AreEqual(3,aggregate.Version);
        }

        [Test]
        public void Should_throw_concurrency_exception()
        {
            var id = Guid.NewGuid();
            Assert.Throws<ConcurrencyException>(() => _session.Get<TestAggregate>(id, 1));
        }

    }
}
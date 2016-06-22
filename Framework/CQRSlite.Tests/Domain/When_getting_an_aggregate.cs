using System;
using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Domain
{
    public class When_getting_an_aggregate
    {
	    private ISession _session;

        public When_getting_an_aggregate()
        {
            var eventStore = new TestEventStore();
            _session = new Session(new Repository(eventStore));
        }

        [Fact]
        public void Should_get_aggregate_from_eventstore()
        {
            var aggregate = _session.Get<TestAggregate>(Guid.NewGuid());
            Assert.NotNull(aggregate);
        }

        [Fact]
        public void Should_apply_events()
        {
            var aggregate = _session.Get<TestAggregate>(Guid.NewGuid());
            Assert.Equal(2,aggregate.DidSomethingCount);
        }

        [Fact]
        public void Should_fail_if_aggregate_do_not_exist()
        {
            Assert.Throws<AggregateNotFoundException>(() => _session.Get<TestAggregate>(Guid.Empty));
        }

        [Fact]
	    public void Should_track_changes()
	    {
            var agg = new TestAggregate(Guid.NewGuid());
            _session.Add(agg);
            var aggregate = _session.Get<TestAggregate>(agg.Id);
            Assert.Equal(agg,aggregate);
	    }

        [Fact]
        public void Should_get_from_session_if_tracked()
        {
            var id = Guid.NewGuid();
            var aggregate = _session.Get<TestAggregate>(id);
            var aggregate2 = _session.Get<TestAggregate>(id);

            Assert.Equal(aggregate, aggregate2);
        }

        [Fact]
        public void Should_throw_concurrency_exception_if_tracked()
        {
            var id = Guid.NewGuid();
            _session.Get<TestAggregate>(id);

            Assert.Throws<ConcurrencyException>(() => _session.Get<TestAggregate>(id, 100));
        }

        [Fact]
        public void Should_get_correct_version()
        {
            var id = Guid.NewGuid();
            var aggregate = _session.Get<TestAggregate>(id);

            Assert.Equal(3,aggregate.Version);
        }

        [Fact]
        public void Should_throw_concurrency_exception()
        {
            var id = Guid.NewGuid();
            Assert.Throws<ConcurrencyException>(() => _session.Get<TestAggregate>(id, 1));
        }
    }
}
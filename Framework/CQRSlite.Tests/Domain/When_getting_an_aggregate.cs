using System;
using System.Threading.Tasks;
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
            var aggregate = _session.Get<TestAggregate>(GuidIdentity.Create());
            Assert.NotNull(aggregate);
        }

        [Fact]
        public async Task Should_apply_events()
        {
            var aggregate = await _session.Get<TestAggregate>(GuidIdentity.Create());
            Assert.Equal(2,aggregate.DidSomethingCount);
        }

        [Fact]
        public async Task Should_fail_if_aggregate_do_not_exist()
        {
            await Assert.ThrowsAsync<AggregateNotFoundException>(async () => await _session.Get<TestAggregate>(new GuidIdentity(Guid.Empty)));
        }

        [Fact]
	    public async Task Should_track_changes()
	    {
            var agg = new TestAggregate(GuidIdentity.Create());
            await _session.Add(agg);
            var aggregate = await _session.Get<TestAggregate>(agg.Id);
            Assert.Equal(agg,aggregate);
	    }

        [Fact]
        public async Task Should_get_from_session_if_tracked()
        {
            var id = GuidIdentity.Create();
            var aggregate = await _session.Get<TestAggregate>(id);
            var aggregate2 = await _session.Get<TestAggregate>(id);

            Assert.Equal(aggregate, aggregate2);
        }

        [Fact]
        public async Task Should_throw_concurrency_exception_if_tracked()
        {
            var id = GuidIdentity.Create();
            await _session.Get<TestAggregate>(id);

            await Assert.ThrowsAsync<ConcurrencyException>(async () => await _session.Get<TestAggregate>(id, 100));
        }

        [Fact]
        public async Task Should_get_correct_version()
        {
            var id = GuidIdentity.Create();
            var aggregate = await _session.Get<TestAggregate>(id);

            Assert.Equal(3,aggregate.Version);
        }

        [Fact]
        public async Task Should_throw_concurrency_exception()
        {
            var id = GuidIdentity.Create();
            await Assert.ThrowsAsync<ConcurrencyException>(async () => await _session.Get<TestAggregate>(id, 1));
        }
    }
}
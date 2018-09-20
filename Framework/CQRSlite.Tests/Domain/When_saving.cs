using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Domain
{
    public class When_saving
    {
        private readonly TestInMemoryEventStore _eventStore;
        private readonly TestAggregateNoParameterLessConstructor _aggregate;
	    private readonly ISession _session;

        public When_saving()
        {
            _eventStore = new TestInMemoryEventStore();
            var rep = new Repository(_eventStore);
            _session = new Session(rep);

            _aggregate = new TestAggregateNoParameterLessConstructor(2);
        }

        [Fact]
        public void Should_save_uncommitted_changes()
        {
            _aggregate.DoSomething();
            _session.Add(_aggregate);
            _session.Commit();
            Assert.Single(_eventStore.Events);
        }

        [Fact]
        public void Should_mark_committed_after_commit()
        {
            _aggregate.DoSomething();
            _session.Add(_aggregate);
            _session.Commit();
            Assert.Empty(_aggregate.GetUncommittedChanges());
        }
        
        [Fact]
        public async Task Should_add_new_aggregate()
        {
            var agg = new TestAggregateNoParameterLessConstructor(1);
            agg.DoSomething();
            await _session.Add(agg);
            await _session.Commit();
            Assert.Single(_eventStore.Events);
        }

        [Fact]
        public async Task Should_set_date()
        {
            var agg = new TestAggregateNoParameterLessConstructor(1);
            agg.DoSomething();
            await _session.Add(agg);
            await _session.Commit();
            Assert.InRange(_eventStore.Events.First().TimeStamp, DateTimeOffset.UtcNow.AddSeconds(-1), DateTimeOffset.UtcNow.AddSeconds(1));
        }

        [Fact]
        public async Task Should_set_version()
        {
            var agg = new TestAggregateNoParameterLessConstructor(1);
            agg.DoSomething();
            agg.DoSomething();
            await _session.Add(agg);
            await _session.Commit();
            Assert.Equal(1, _eventStore.Events.First().Version);
            Assert.Equal(2, _eventStore.Events.Last().Version);
        }

        [Fact]
        public async Task Should_set_id()
        {
            var id = Guid.NewGuid();
            var agg = new TestAggregateNoParameterLessConstructor(1, id);
            agg.DoSomething();
            await _session.Add(agg);
            await _session.Commit();
            Assert.Equal(id, _eventStore.Events.First().Id);
        }

        [Fact]
        public async Task Should_clear_tracked_aggregates()
        {
            var agg = new TestAggregate(Guid.NewGuid());
            await _session.Add(agg);
            agg.DoSomething();
            await _session.Commit();
            _eventStore.Events.Clear();

            await Assert.ThrowsAsync<AggregateNotFoundException>(async () => await _session.Get<TestAggregate>(agg.Id));
        }

        [Fact]
        public async Task Should_forward_cancellation_token()
        {
            var token = new CancellationToken();
            var agg = new TestAggregate(Guid.NewGuid());
            await _session.Add(agg, token);
            agg.DoSomething();
            await _session.Commit(token);
            Assert.Equal(token, _eventStore.Token);
        }
    }
}

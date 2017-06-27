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
        private TestInMemoryEventStore _eventStore;
        private TestAggregateNoParameterLessConstructor _aggregate;
	    private ISession _session;
	    private Repository _rep;

        public When_saving()
        {
            _eventStore = new TestInMemoryEventStore();
            _rep = new Repository(_eventStore);
            _session = new Session(_rep);

            _aggregate = new TestAggregateNoParameterLessConstructor(2);
        }

        [Fact]
        public void Should_save_uncommited_changes()
        {
            _aggregate.DoSomething();
            _session.Add(_aggregate);
            _session.Commit();
            Assert.Equal(1, _eventStore.Events.Count);
        }

        [Fact]
        public void Should_mark_commited_after_commit()
        {
            _aggregate.DoSomething();
            _session.Add(_aggregate);
            _session.Commit();
            Assert.Equal(0, _aggregate.GetUncommittedChanges().Length);
        }
        
        [Fact]
        public async Task Should_add_new_aggregate()
        {
            var agg = new TestAggregateNoParameterLessConstructor(1);
            agg.DoSomething();
            await _session.Add(agg);
            await _session.Commit();
            Assert.Equal(1, _eventStore.Events.Count);
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

using System;
using System.Linq;
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
            Assert.Equal(0, _aggregate.GetUncommittedChanges().Count());
        }
        
        [Fact]
        public void Should_add_new_aggregate()
        {
            var agg = new TestAggregateNoParameterLessConstructor(1);
            agg.DoSomething();
            _session.Add(agg);
            _session.Commit();
            Assert.Equal(1, _eventStore.Events.Count);
        }

        [Fact]
        public void Should_set_date()
        {
            var agg = new TestAggregateNoParameterLessConstructor(1);
            agg.DoSomething();
            _session.Add(agg);
            _session.Commit();
            Assert.InRange(_eventStore.Events.First().TimeStamp, DateTimeOffset.UtcNow.AddSeconds(-1), DateTimeOffset.UtcNow.AddSeconds(1));
        }

        [Fact]
        public void Should_set_version()
        {
            var agg = new TestAggregateNoParameterLessConstructor(1);
            agg.DoSomething();
            agg.DoSomething();
            _session.Add(agg);
            _session.Commit();
            Assert.Equal(1, _eventStore.Events.First().Version);
            Assert.Equal(2, _eventStore.Events.Last().Version);
        }

        [Fact]
        public void Should_set_id()
        {
            var id = Guid.NewGuid();
            var agg = new TestAggregateNoParameterLessConstructor(1, id);
            agg.DoSomething();
            _session.Add(agg);
            _session.Commit();
            Assert.Equal(id, _eventStore.Events.First().Id);
        }

        [Fact]
        public void Should_clear_tracked_aggregates()
        {
            var agg = new TestAggregate(Guid.NewGuid());
            _session.Add(agg);
            agg.DoSomething();
            _session.Commit();
            _eventStore.Events.Clear();

            Assert.Throws<AggregateNotFoundException>(() => _session.Get<TestAggregate>(agg.Id));
        }
    }
}

using System;
using System.Linq;
using CQRSlite.Domain;
using CQRSlite.Eventing;
using CQRSlite.Infrastructure;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.DomainTests
{
	[TestFixture]
    public class When_saving
    {
        private TestEventStore _eventStore;
        private TestAggregateNoParameterLessConstructor _aggregate;
        private TestEventPublisher _eventPublisher;
        private Repository<TestAggregateNoParameterLessConstructor> _rep;
	    private Session _session;

	    [SetUp]
        public void Setup()
        {
            _eventStore = new TestEventStore();
            _eventPublisher = new TestEventPublisher();
            var snapshotstore = new NullSnapshotStore();
            var snapshotStrategy = new DefaultSnapshotStrategy();
		    _session = new Session(_eventStore, snapshotstore, _eventPublisher, snapshotStrategy);
            _rep = new Repository<TestAggregateNoParameterLessConstructor>(_session, _eventStore, snapshotstore, snapshotStrategy);
            _aggregate = new TestAggregateNoParameterLessConstructor(2);

        }

        [Test]
        public void Should_save_uncommited_changes()
        {
            _aggregate.DoSomething();
            _rep.Add(_aggregate);
            _session.Commit();
            Assert.AreEqual(1, _eventStore.SavedEvents.Count);
        }

        [Test]
        public void Should_mark_commited_after_commit()
        {
            _aggregate.DoSomething();
            _rep.Add(_aggregate);
            _session.Commit();
            Assert.AreEqual(0, _aggregate.GetUncommittedChanges().Count());
        }

        [Test]
        public void Should_throw_concurrency_exception()
        {
            _aggregate.SetVersion(12);
            _rep.Add(_aggregate);

            Assert.Throws<ConcurrencyException>(() => _session.Commit());
        }
        
        [Test]
        public void Should_publish_events()
        {
            _aggregate.DoSomething();
            _rep.Add(_aggregate);
            _session.Commit();
            Assert.AreEqual(1, _eventPublisher.Published);
        }

        [Test]
        public void Should_add_new_aggregate()
        {
            var agg = new TestAggregateNoParameterLessConstructor(1,Guid.Empty);
            agg.DoSomething();
            _rep.Add(agg);
            _session.Commit();
            Assert.AreEqual(1, _eventStore.SavedEvents.Count);
        }

        [Test]
        public void Should_set_date()
        {
            var agg = new TestAggregateNoParameterLessConstructor(1, Guid.Empty);
            agg.DoSomething();
            _rep.Add(agg);
            _session.Commit();
            Assert.That(_eventStore.SavedEvents.First().TimeStamp, Is.InRange(DateTimeOffset.UtcNow.AddSeconds(-1), DateTimeOffset.UtcNow.AddSeconds(1)));
        }
    }
}

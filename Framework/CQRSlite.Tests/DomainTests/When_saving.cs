using System;
using System.Linq;
using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using CQRSlite.Infrastructure.Repositories.Domain;
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
	    private ISession _session;
	    private Repository _rep;

	    [SetUp]
        public void Setup()
        {
            _eventStore = new TestEventStore();
            _eventPublisher = new TestEventPublisher();
	        _rep = new Repository(_eventStore, _eventPublisher);
            _session = new Session(_rep);

            _aggregate = new TestAggregateNoParameterLessConstructor(2);

        }

        [Test]
        public void Should_save_uncommited_changes()
        {
            _aggregate.DoSomething();
            _session.Add(_aggregate);
            _session.Commit();
            Assert.AreEqual(1, _eventStore.SavedEvents.Count);
        }

        [Test]
        public void Should_mark_commited_after_commit()
        {
            _aggregate.DoSomething();
            _session.Add(_aggregate);
            _session.Commit();
            Assert.AreEqual(0, _aggregate.GetUncommittedChanges().Count());
        }
        
        [Test]
        public void Should_publish_events()
        {
            _aggregate.DoSomething();
            _session.Add(_aggregate);
            _session.Commit();
            Assert.AreEqual(1, _eventPublisher.Published);
        }

        [Test]
        public void Should_add_new_aggregate()
        {
            var agg = new TestAggregateNoParameterLessConstructor(1,Guid.Empty);
            agg.DoSomething();
            _session.Add(agg);
            _session.Commit();
            Assert.AreEqual(1, _eventStore.SavedEvents.Count);
        }

        [Test]
        public void Should_set_date()
        {
            var agg = new TestAggregateNoParameterLessConstructor(1, Guid.Empty);
            agg.DoSomething();
            _session.Add(agg);
            _session.Commit();
            Assert.That(_eventStore.SavedEvents.First().TimeStamp, Is.InRange(DateTimeOffset.UtcNow.AddSeconds(-1), DateTimeOffset.UtcNow.AddSeconds(1)));
        }

        [Test]
        public void Should_set_version()
        {
            var agg = new TestAggregateNoParameterLessConstructor(1, Guid.Empty);
            agg.DoSomething();
            _session.Add(agg);
            _session.Commit();
            Assert.That(_eventStore.SavedEvents.First().Version, Is.EqualTo(1));
        }

        [Test]
        public void ShouldThrowConcurrencyException()
        {
            Assert.Throws<ConcurrencyException>(() => _rep.Save(_aggregate, 1));
        }
    }
}

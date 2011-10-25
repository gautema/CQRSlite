using System;
using CQRSlite.Domain;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.DomainTests
{
	[TestFixture]
    public class When_getting_aggregate_without_contructor
    {
        private Repository<TestAggregateNoParameterLessConstructor> _rep;
	    private TestAggregateNoParameterLessConstructor _aggregate;

	    [SetUp]
        public void Setup()
        {
            var eventStore = new TestEventStore();
            var eventPublisher = new TestEventPublisher();
            var session = new Session(eventStore, null, eventPublisher);
            _rep = new Repository<TestAggregateNoParameterLessConstructor>(session, eventStore, null);
            _aggregate = _rep.Get(Guid.NewGuid());
        }

        [Test]
        public void Should_create_aggregate()
        {
            Assert.That(_aggregate, Is.Not.Null);
        }

        [Test]
        public void Should_playback_events()
        {
            Assert.That(_aggregate.Version,Is.EqualTo(3));
        }


    }
}
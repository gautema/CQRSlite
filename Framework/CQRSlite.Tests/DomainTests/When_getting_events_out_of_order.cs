using System;
using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.DomainTests
{
	[TestFixture]
    public class When_getting_events_out_of_order
    {
	    private ISession _session;

	    [SetUp]
        public void Setup()
        {
            var eventStore = new TestEventStoreWithBugs();
            var testEventPublisher = new TestEventPublisher();
            _session = new Session(new Repository(eventStore, testEventPublisher));
        }

        [Test]
        public void Should_throw_concurrency_exception()
        {
            var id = Guid.NewGuid();
            Assert.Throws<EventsOutOfOrderException>(() => _session.Get<TestAggregate>(id, 3));
        }
    }
}
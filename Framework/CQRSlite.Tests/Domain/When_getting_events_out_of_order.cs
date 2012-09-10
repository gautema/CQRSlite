using System;
using CQRSlite.Contracts.Infrastructure.Repositories;
using CQRSlite.Domain.Exception;
using CQRSlite.Infrastructure.Repositories.Domain;
using CQRSlite.Tests.Substitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.Domain
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
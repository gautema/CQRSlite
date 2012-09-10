using System;
using CQRSlite.Cache;
using CQRSlite.Contracts.Domain.Exception;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.CachingTests
{
    [TestFixture]
    public class When_getting_earlier_than_expected_events_from_event_store
    {
        private CacheRepository _rep;
        private TestAggregate _aggregate;

        [SetUp]
        public void Setup()
        {
            _rep = new CacheRepository(new TestRepository(), new TestEventStoreWithBugs());
            _aggregate = _rep.Get<TestAggregate>(Guid.NewGuid());
        }

        [Test]
        public void Should_throw_out_of_order_execption()
        {
            Assert.Throws<EventsOutOfOrderException>(() => _rep.Get<TestAggregate>(_aggregate.Id));

        }
    }
}
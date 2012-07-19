using System;
using CQRSlite.Caching;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.CachingTests
{
    [TestFixture]
    public class When_getting_aggregate
    {
        private CachingRepository _rep;
        private TestAggregate _aggregate;

        [SetUp]
        public void Setup()
        {
            _rep = new CachingRepository(new TestRepository(), new TestEventStore());
            _aggregate = _rep.Get<TestAggregate>(Guid.NewGuid());
        }

        [Test]
        public void Should_get_aggregate()
        {
            Assert.That(_aggregate, Is.Not.Null);
        }

        [Test]
        public void Should_get_same_aggregate_on_second_try()
        {
            var aggregate =_rep.Get<TestAggregate>(_aggregate.Id);
            Assert.That(aggregate, Is.EqualTo(_aggregate));
        }

        [Test]
        public void Should_update_if_version_changed_in_event_store()
        {
            var aggregate = _rep.Get<TestAggregate>(_aggregate.Id);
            Assert.That(aggregate.Version, Is.EqualTo(3));
        }
    }
}
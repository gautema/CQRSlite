using System;
using CQRSlite.Cache;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.CachingTests
{
    [TestFixture]
    public class When_saving_aggregate
    {
        private CacheRepository _rep;
        private TestAggregate _aggregate;
        private TestRepository _testRep;

        [SetUp]
        public void Setup()
        {
            _testRep = new TestRepository();
            _rep = new CacheRepository(_testRep, new TestEventStore());
            _aggregate = _testRep.Get<TestAggregate>(Guid.NewGuid());
            _aggregate.DoSomething();
            _rep.Save(_aggregate,-1);
        }

        [Test]
        public void Should_get_same_aggregate_on_get()
        {
            var aggregate = _rep.Get<TestAggregate>(_aggregate.Id);
            Assert.That(aggregate, Is.EqualTo(_aggregate));
        }

        [Test]
        public void Should_save_to_repository()
        {
            Assert.That(_testRep.Saved, Is.EqualTo(_aggregate));
        }
    }
}
using System;
using CQRSlite.Domain;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.DomainTests
{
	[TestFixture]
    public class WhenGettingNotValidAggregate
    {
        private Repository<TestAggregateNoParameterLessConstructor> _rep;

		[SetUp]
        public void Setup()
        {
            var eventStore = new TestEventStore();
            var eventPublisher = new TestEventPublisher();
            _rep = new Repository<TestAggregateNoParameterLessConstructor>(eventStore, null, eventPublisher);
        }

        [Test]
        public void ShouldThrowIfNoParameterlessConstructor()
        {

            Assert.Throws<AggreagateMissingParameterlessConstructorException>(() =>
                                                                                  {
                                                                                      _rep.Get(Guid.NewGuid());
                                                                                  });

        }
    }
}
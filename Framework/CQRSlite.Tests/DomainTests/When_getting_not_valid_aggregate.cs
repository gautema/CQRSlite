using System;
using CQRSlite.Domain;
using CQRSlite.Infrastructure;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.DomainTests
{
	[TestFixture]
    public class When_getting_not_valid_aggregate
    {
        private Repository<TestAggregateNoParameterLessConstructor> _rep;

		[SetUp]
        public void Setup()
        {
            var eventStore = new TestEventStore();
            var eventPublisher = new TestEventPublisher();
            var snapshotStrategy = new DefaultSnapshotStrategy();
            _rep = new Repository<TestAggregateNoParameterLessConstructor>(eventStore, null, eventPublisher, snapshotStrategy);
        }

        [Test]
        public void Should_throw_if_no_parameterless_constructor()
        {

            Assert.Throws<AggregateMissingParameterlessConstructorException>(() =>
                                                                                  {
                                                                                      _rep.Get(Guid.NewGuid());
                                                                                  });

        }
    }
}
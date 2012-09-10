using System;
using CQRSlite.Contracts.Domain;
using CQRSlite.Contracts.Domain.Exception;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.DomainTests
{
	[TestFixture]
    public class When_getting_aggregate_without_contructor
    {
	    private ISession _session;

	    [SetUp]
        public void Setup()
        {
            var eventStore = new TestEventStore();
            var eventPublisher = new TestEventPublisher();
            _session = new Session(new Repository(eventStore, eventPublisher));
        }

        [Test]
        public void Should_throw_missing_parameterless_constructor_exception()
        {
            Assert.Throws<MissingParameterLessConstructorException>(() => _session.Get<TestAggregateNoParameterLessConstructor>(Guid.NewGuid()));
        }
    }
}
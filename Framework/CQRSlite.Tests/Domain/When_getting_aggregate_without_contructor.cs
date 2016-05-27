using System;
using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using CQRSlite.Tests.Substitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.Domain
{
	[TestFixture]
    public class When_getting_aggregate_without_contructor
    {
	    private Guid _id;
	    private Repository _repository;

	    [SetUp]
        public void Setup()
	    {
	        _id = Guid.NewGuid();
            var eventStore = new TestInMemoryEventStore();
            _repository = new Repository(eventStore);
	        var aggreagate = new TestAggregateNoParameterLessConstructor(1, _id);
            aggreagate.DoSomething();
            _repository.Save(aggreagate);
	    }

        [Test]
        public void Should_throw_missing_parameterless_constructor_exception()
        {
            Assert.Throws<MissingParameterLessConstructorException>(() => _repository.Get<TestAggregateNoParameterLessConstructor>(_id));
        }
    }
}
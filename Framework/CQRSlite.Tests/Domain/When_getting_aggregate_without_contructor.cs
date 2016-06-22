using System;
using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Domain
{
    public class When_getting_aggregate_without_contructor
    {
	    private Guid _id;
	    private Repository _repository;

        public When_getting_aggregate_without_contructor()
        {
            _id = Guid.NewGuid();
            var eventStore = new TestInMemoryEventStore();
            _repository = new Repository(eventStore);
            var aggreagate = new TestAggregateNoParameterLessConstructor(1, _id);
            aggreagate.DoSomething();
            _repository.Save(aggreagate);
        }

        [Fact]
        public void Should_throw_missing_parameterless_constructor_exception()
        {
            Assert.Throws<MissingParameterLessConstructorException>(() => _repository.Get<TestAggregateNoParameterLessConstructor>(_id));
        }
    }
}
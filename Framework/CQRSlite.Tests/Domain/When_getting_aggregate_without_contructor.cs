using System;
using System.Threading.Tasks;
using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Domain
{
    public class When_getting_aggregate_without_contructor
    {
	    private readonly Guid _id;
	    private readonly Repository _repository;

        public When_getting_aggregate_without_contructor()
        {
            _id = Guid.NewGuid();
            var eventStore = new TestInMemoryEventStore();
            _repository = new Repository(eventStore);
            var aggregate = new TestAggregateNoParameterLessConstructor(1, _id);
            aggregate.DoSomething();
            Task.Run(() => _repository.Save(aggregate)).Wait();
        }

        [Fact]
        public async Task Should_throw_missing_parameterless_constructor_exception()
        {
            await Assert.ThrowsAsync<MissingParameterLessConstructorException>(async () => await _repository.Get<TestAggregateNoParameterLessConstructor>(_id));
        }
    }
}
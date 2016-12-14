using System;
using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using CQRSlite.Tests.Substitutes;
using Xunit;
using System.Threading.Tasks;

namespace CQRSlite.Tests.Domain
{
    public class When_async_getting_aggregate_without_contructor
    {
        private Guid _id;
        private Repository _repository;

        public When_async_getting_aggregate_without_contructor()
        {
            _id = Guid.NewGuid();
            var eventStore = new TestInMemoryEventStore();
            _repository = new Repository(eventStore);
        }

        [Fact]
        public async Task Should_throw_missing_parameterless_constructor_exception()
        {
            var aggreagate = new TestAggregateNoParameterLessConstructor(1, _id);
            aggreagate.DoSomething();
            await _repository.SaveAsync(aggreagate);

            await Assert.ThrowsAsync<MissingParameterLessConstructorException>(async () => await _repository.GetAsync<TestAggregateNoParameterLessConstructor>(_id));
        }
    }
}

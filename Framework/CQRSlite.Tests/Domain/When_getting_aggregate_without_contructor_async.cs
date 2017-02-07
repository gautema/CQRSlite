using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using CQRSlite.Tests.Substitutes;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CQRSlite.Tests.Domain
{
    public class When_getting_aggregate_without_contructor_async
    {
        private Guid _id;
        private Repository _repository;

        public When_getting_aggregate_without_contructor_async()
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

using System;
using System.Threading.Tasks;
using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Domain
{
    public class When_saving_events_with_wrong_id
    {
        private readonly TestAggregate _aggregate;
        private readonly Repository _rep;

        public When_saving_events_with_wrong_id()
        {
            var eventStore = new TestInMemoryEventStore();
            _rep = new Repository(eventStore);

            _aggregate = new TestAggregate(Guid.NewGuid());
            _aggregate.ApplyChangeProxy(new TestAggregateDidSomething{Id = Guid.NewGuid()});
        }

        [Fact]
        public async Task Should_throw()
        {
            await Assert.ThrowsAsync<EventIdIncorrectException>(async () => await _rep.Save(_aggregate, 0));
        }
    }
}
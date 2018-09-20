using System;
using System.Threading.Tasks;
using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Domain
{
    public class When_getting_events_out_of_order
    {
	    private readonly ISession _session;

        public When_getting_events_out_of_order()
        {
            var eventStore = new TestEventStoreWithBugs();
            _session = new Session(new Repository(eventStore));
        }

        [Fact]
        public async Task Should_throw_concurrency_exception()
        {
            var id = Guid.NewGuid();
            await Assert.ThrowsAsync<EventsOutOfOrderException>(async () => await _session.Get<TestAggregate>(id, 3));
        }
    }
}
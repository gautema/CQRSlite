using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using CQRSlite.Events;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Domain
{
    public class When_getting_events_with_wrong_id
    {
	    private readonly ISession _session;

        public When_getting_events_with_wrong_id()
        {
            var eventStore = new Store();
            _session = new Session(new Repository(eventStore));
        }

        
        class Store : IEventStore
        {
            public Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new List<IEvent>
                {
                    new TestAggregateDidSomething {Id = aggregateId, Version = 1},
                    new TestAggregateDidSomethingElse {Id = Guid.NewGuid(), Version = 2},
                    new TestAggregateDidSomething {Id = aggregateId, Version = 3},
                }.Where(x => x.Version > fromVersion));
            }
        }
        
        [Fact]
        public async Task Should_throw()
        {
            var id = Guid.NewGuid();
            await Assert.ThrowsAsync<EventIdIncorrectException>(async () => await _session.Get<TestAggregate>(id, 3));
        }
    }
}
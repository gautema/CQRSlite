using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CQRSlite.Events;

namespace CQRSlite.Tests.Substitutes
{
    public class TestEventStoreWithBugs : IEventStore
    {
        public Task Save<T>(IEnumerable<IEvent> events)
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<IEvent>> Get<T>(Guid aggregateId, int version)
        {
            if (aggregateId == Guid.Empty)
            {
                return Task.FromResult((IEnumerable<IEvent>)new List<IEvent>());
            }

            return Task.FromResult((IEnumerable<IEvent>)new List<IEvent>
            {
                new TestAggregateDidSomething {Id = aggregateId, Version = 3},
                new TestAggregateDidSomething {Id = aggregateId, Version = 2},
                new TestAggregateDidSomeethingElse {Id = aggregateId, Version = 1},
            });
        }
    }
}
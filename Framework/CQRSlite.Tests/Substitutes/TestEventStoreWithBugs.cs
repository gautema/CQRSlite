using System;
using System.Collections.Generic;
using CQRSlite.Events;
using System.Threading.Tasks;

namespace CQRSlite.Tests.Substitutes
{
    public class TestEventStoreWithBugs : IEventStore
    {
        public void Save<T>(IEnumerable<IEvent> events)
        {

        }

        public Task SaveAsync<T>(IEnumerable<IEvent> events)
        {
            return Task.FromResult(0);
        }

        public IEnumerable<IEvent> Get<T>(Guid aggregateId, int version)
        {
            if (aggregateId == Guid.Empty)
            {
                return new List<IEvent>();
            }

            return new List<IEvent>
            {
                new TestAggregateDidSomething {Id = aggregateId, Version = 3},
                new TestAggregateDidSomething {Id = aggregateId, Version = 2},
                new TestAggregateDidSomeethingElse {Id = aggregateId, Version = 1},
            };
        }

        public Task<IEnumerable<IEvent>> GetAsync<T>(Guid aggregateId, int fromVersion)
        {
            return Task.FromResult(Get<T>(aggregateId, fromVersion));
        }
    }
}
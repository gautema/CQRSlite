using System;
using System.Collections.Generic;
using CQRSlite.Events;

namespace CQRSlite.Tests.Substitutes
{
    public class TestEventStoreWithBugs : IEventStore
    {
        public IEnumerable<IEvent> Get(Guid aggregateId, int version)
        {
            if (aggregateId == Guid.Empty)
            {
                return new List<IEvent>();
            }

            return new List<IEvent>
                {
                    new TestAggregateDidSomething {Id = aggregateId, Version = 2},
                    new TestAggregateDidSomeethingElse {Id = aggregateId, Version = 1},
                    new TestAggregateDidSomething {Id = aggregateId, Version = 3},
                };

        }

        public void Save(IEvent eventDescriptor)
        {
        }

    }
}
using System;
using System.Collections.Generic;
using CQRSlite.Eventing;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestEventStore : IEventStore
    {
        public IEnumerable<EventDescriptor> Get(Guid aggregateId, int version)
        {
            if (aggregateId == Guid.Empty)
            {
                return new List<EventDescriptor>();
            }

            return new List<EventDescriptor>
                                   {
                                       new EventDescriptor(aggregateId, new TestAggregateDidSomething(), 1),
                                       new EventDescriptor(aggregateId, new TestAggregateDidSomeethingElse(), 2),
                                       new EventDescriptor(aggregateId, new TestAggregateDidSomething(), 3)
                                   };

        }

        public int GetVersion(Guid aggregateId)
        {
            return aggregateId == Guid.Empty ? 0 : 2;
        }
        public void Save(Guid aggregateId, EventDescriptor eventDescriptors)
        {
            SavedEvents++;
        }

        public int SavedEvents { get; set; }
    }
}

using System;
using System.Collections.Generic;
using CQRSlite.Eventing;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestEventStore : IEventStore
    {
        public IEnumerable<Event> Get(Guid aggregateId, int version)
        {
            if (aggregateId == Guid.Empty)
            {
                return new List<Event>();
            }

            return new List<Event>
                                   {
                                       new TestAggregateDidSomething {Id= aggregateId, Version = 1},
                                       new TestAggregateDidSomeethingElse{Id= aggregateId, Version = 2},
                                       new TestAggregateDidSomething{Id= aggregateId, Version = 3},
                                   };

        }

        public int GetVersion(Guid aggregateId)
        {
            return aggregateId == Guid.Empty ? 0 : 2;
        }
        public void Save(Guid aggregateId, Event eventDescriptor)
        {
            SavedEvents++;
        }

        public int SavedEvents { get; set; }
    }
}

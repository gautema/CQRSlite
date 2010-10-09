using System;
using System.Collections.Generic;
using CQRSlite.Eventing;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestEventRepository : IEventRepository
    {
        public bool TryGetEvents(Guid aggregateId, int version, out List<EventStore.EventDescriptor> eventDescriptors)
        {
            if (aggregateId == Guid.Empty)
            {
                eventDescriptors = new List<EventStore.EventDescriptor>();
                return false;
            }


            eventDescriptors = new List<EventStore.EventDescriptor>
                                   {
                                       new EventStore.EventDescriptor(aggregateId, new TestAggregateDidSomething(), 1),
                                       new EventStore.EventDescriptor(aggregateId, new TestAggregateDidSomeethingElse(), 2)
                                   };
            return true;
        }

        public void Add(Guid aggregateId, List<EventStore.EventDescriptor> eventDescriptors)
        {
            AddedEvents++;
        }

        public void Save(Guid aggregateId, EventStore.EventDescriptor eventDescriptors)
        {
            SavedEvents++;
        }

        public int AddedEvents { get; set; }
        public int SavedEvents { get; set; }
    }
}

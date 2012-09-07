using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Eventing;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestEventStore : IEventStore
    {
        public TestEventStore()
        {
            SavedEvents = new List<Event>();   
        }
        public IEnumerable<Event> Get(Guid aggregateId, int version)
        {
            if (aggregateId == Guid.Empty)
            {
                return new List<Event>();
            }

            return new List<Event>
                {
                    new TestAggregateDidSomething {Id = aggregateId, Version = 1},
                    new TestAggregateDidSomeethingElse {Id = aggregateId, Version = 2},
                    new TestAggregateDidSomething {Id = aggregateId, Version = 3},
                }.Where(x => x.Version > version);

        }

        public int GetVersion(Guid aggregateId)
        {
            return aggregateId == Guid.Empty ? 0 : 2;
        }
        public void Save(Guid aggregateId, Event @event)
        {
            SavedEvents.Add(@event);
        }

        public List<Event> SavedEvents { get; set; }
    }
}

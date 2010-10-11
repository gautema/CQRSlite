using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Eventing;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestEventStore : IEventStore
    {
        public int SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion)
        {
            SavedEvents = events.Count();
            return expectedVersion + events.Count();
        }

        public IEnumerable<Event> GetEventsForAggregate(Guid aggregateId, int fromVersion)
        {
            return new List<Event> {new TestAggregateDidSomething(), new TestAggregateDidSomething(), new TestAggregateDidSomeethingElse {Version = 3}};
        }

        public int SavedEvents { get; set; }
    }
}

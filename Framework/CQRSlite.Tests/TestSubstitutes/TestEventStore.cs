using System;
using System.Collections.Generic;
using System.Linq;
using SimpleCQRS.Eventing;

namespace SimpleCQRS.Tests.TestSubstitutes
{
    public class TestEventStore : IEventStore
    {
        public void SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion)
        {
            SavedEvents = events.Count();
        }

        public IEnumerable<Event> GetEventsForAggregate(Guid aggregateId)
        {
            return new List<Event> {new TestAggregateDidSomething(), new TestAggregateDidSomething(), new TestAggregateDidSomeethingElse()};
        }

        public int SavedEvents { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Eventing;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestEventStore : IEventStore
    {
        public void SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion)
        {
            SavedEvents = events.Count();
        }

        public IEnumerable<Event> GetEventsForAggregate(Guid aggregateId, int fromVersion)
        {
            return new List<Event> {new TestAggregateDidSomething(), new TestAggregateDidSomething(), new TestAggregateDidSomeethingElse()};
        }

        public int SavedEvents { get; set; }
    }
}

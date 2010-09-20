using System;
using System.Collections.Generic;
using SimpleCQRS.Events;

namespace SimpleCQRS.EventStore
{
    public interface IEventStore
    {
        void SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion);
        List<Event> GetEventsForAggregate(Guid aggregateId);
    }
}
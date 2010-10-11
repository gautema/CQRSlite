using System;
using System.Collections.Generic;

namespace CQRSlite.Eventing
{
    public interface IEventStore
    {
        int SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion);
        IEnumerable<Event> GetEventsForAggregate(Guid aggregateId, int fromVersion);
    }
}
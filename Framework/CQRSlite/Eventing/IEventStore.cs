using System;
using System.Collections.Generic;

namespace CQRSlite.Eventing
{
    public interface IEventStore
    {
        void SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion);
        IEnumerable<Event> GetEventsForAggregate(Guid aggregateId, int version);
    }
}
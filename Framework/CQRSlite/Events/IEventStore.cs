using System;
using System.Collections.Generic;

namespace CQRSlite.Events
{
    public interface IEventStore 
    {
        void Save(Guid aggregateId, Event @event);
        IEnumerable<Event> Get(Guid aggregateId, int fromVersion);
        int GetVersion(Guid aggregateId);
    }
}
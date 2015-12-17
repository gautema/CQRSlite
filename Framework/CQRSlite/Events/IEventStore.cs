using System;
using System.Collections.Generic;

namespace CQRSlite.Events
{
    public interface IEventStore 
    {
        void Save(IEnumerable<IEvent> events);
        IEnumerable<IEvent> Get(Guid aggregateId, int fromVersion);
    }
}
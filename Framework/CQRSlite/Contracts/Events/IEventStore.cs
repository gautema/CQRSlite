using System;
using System.Collections.Generic;
using CQRSlite.Contracts.Messages;

namespace CQRSlite.Contracts.Events
{
    public interface IEventStore 
    {
        void Save(Guid aggregateId, Event @event);
        IEnumerable<Event> Get(Guid aggregateId, int fromVersion);
        int GetVersion(Guid aggregateId);
    }
}
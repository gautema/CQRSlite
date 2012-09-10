using System;
using System.Collections.Generic;
using CQRSlite.Contracts.Bus.Messages;

namespace CQRSlite.Contracts.Bus.Events
{
    public interface IEventStore 
    {
        void Save(Guid aggregateId, Event @event);
        IEnumerable<Event> Get(Guid aggregateId, int fromVersion);
        int GetVersion(Guid aggregateId);
    }
}
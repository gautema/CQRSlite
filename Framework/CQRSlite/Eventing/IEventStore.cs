using System;
using System.Collections.Generic;

namespace CQRSlite.Eventing
{
    public interface IEventStore 
    {
        void Save(Guid aggregateId, EventDescriptor eventDescriptors);
        IEnumerable<EventDescriptor> Get(Guid aggregateId, int fromVersion);
        int GetVersion(Guid aggregateId);
    }
}
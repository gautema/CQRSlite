using System;
using System.Collections.Generic;

namespace CQRSlite.Eventing
{
    public interface IEventRepository {
        void Save(Guid aggregateId, EventStore.EventDescriptor eventDescriptors);
        bool TryGetEvents(Guid aggregateId, int fromVersion, out List<EventStore.EventDescriptor> list);
        int GetVersion(Guid aggregateId);
    }
}
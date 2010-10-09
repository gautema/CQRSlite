using System;
using System.Collections.Generic;

namespace CQRSlite.Eventing
{
    public interface IEventRepository {
        void Add(Guid aggregateId, List<EventStore.EventDescriptor> eventDescriptors);
        void Save(Guid aggregateId, EventStore.EventDescriptor eventDescriptors);
        bool TryGetEvents(Guid aggregateId, int eventDescriptors, out List<EventStore.EventDescriptor> list);
    }
}
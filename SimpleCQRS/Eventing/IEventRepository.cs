using System;
using System.Collections.Generic;

namespace SimpleCQRS.Eventing
{
    public interface IEventRepository {
        bool TryGetEvents(Guid aggregateId, out List<EventStore.EventDescriptor> eventDescriptors);
        void Add(Guid aggregateId, List<EventStore.EventDescriptor> eventDescriptors);
        void Save(Guid aggregateId, EventStore.EventDescriptor eventDescriptors);
    }
}
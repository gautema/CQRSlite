using System;
using System.Collections.Generic;
using CQRSlite.Eventing;

namespace CQRSCode.Domain
{
    public class EventRepository : IEventRepository
    {
        private Dictionary<Guid, List<EventStore.EventDescriptor>> db = new Dictionary<Guid, List<EventStore.EventDescriptor>>();

        public bool TryGetEvents(Guid aggregateId, out List<EventStore.EventDescriptor> eventDescriptors)
        {
            return db.TryGetValue(aggregateId, out eventDescriptors);
        }

        public void Add(Guid aggregateId, List<EventStore.EventDescriptor> eventDescriptors)
        {
            db.Add(aggregateId, eventDescriptors);
        }

        public void Save(Guid aggregateId, EventStore.EventDescriptor eventDescriptors)
        {
            List<EventStore.EventDescriptor> list;
            db.TryGetValue(aggregateId, out list);
            list.Add(eventDescriptors);
        }
    }
}

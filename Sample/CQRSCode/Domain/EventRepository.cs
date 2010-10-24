using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Eventing;

namespace CQRSCode.Domain
{
    public class EventRepository : IEventRepository
    {
        private Dictionary<Guid, List<EventStore.EventDescriptor>> db = new Dictionary<Guid, List<EventStore.EventDescriptor>>();

        public bool TryGetEvents(Guid aggregateId, int version, out List<EventStore.EventDescriptor> eventDescriptors)
        {
            return db.TryGetValue(aggregateId, out eventDescriptors);
        }

        public int GetVersion(Guid aggregateId)
        {
            List<EventStore.EventDescriptor> eventDescriptors;
            db.TryGetValue(aggregateId, out eventDescriptors);
            if (eventDescriptors == null)
                return 0;
            return eventDescriptors.Max(x => x.Version);
        }

        public void Save(Guid aggregateId, EventStore.EventDescriptor eventDescriptors)
        {
            List<EventStore.EventDescriptor> list;
            db.TryGetValue(aggregateId, out list);
            if(list == null)
            {
                list = new List<EventStore.EventDescriptor>();
                db.Add(aggregateId, list);
            }
            list.Add(eventDescriptors);
        }
    }
}

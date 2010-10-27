using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Eventing;

namespace CQRSCode.Domain
{
    public class EventStore : IEventStore
    {
        private Dictionary<Guid, List<EventDescriptor>> db = new Dictionary<Guid, List<EventDescriptor>>();

        public IEnumerable<EventDescriptor> Get(Guid aggregateId, int fromVersion)
        {
            List<EventDescriptor> alldescriptors;
            db.TryGetValue(aggregateId, out alldescriptors);
            return alldescriptors.Where(x => x.Version > fromVersion);
        }

        public int GetVersion(Guid aggregateId)
        {
            List<EventDescriptor> eventDescriptors;
            db.TryGetValue(aggregateId, out eventDescriptors);
            if (eventDescriptors == null)
                return 0;
            return eventDescriptors.Max(x => x.Version);
        }

        public void Save(Guid aggregateId, EventDescriptor eventDescriptors)
        {
            List<EventDescriptor> list;
            db.TryGetValue(aggregateId, out list);
            if(list == null)
            {
                list = new List<EventDescriptor>();
                db.Add(aggregateId, list);
            }
            list.Add(eventDescriptors);
        }
    }
}

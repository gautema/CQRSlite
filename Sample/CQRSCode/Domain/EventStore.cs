using System;
using System.Collections.Generic;
using System.Linq;
using CQRSCode.Events;
using CQRSlite.Eventing;

namespace CQRSCode.Domain
{
    public class EventStore : IEventStore
    {
        private readonly Dictionary<Guid, List<EventDescriptor>> db = new Dictionary<Guid, List<EventDescriptor>>();

        public IEnumerable<Event> Get(Guid aggregateId, int fromVersion)
        {
            List<EventDescriptor> alldescriptors;
            db.TryGetValue(aggregateId, out alldescriptors);
            return alldescriptors.Where(x => x.Version > fromVersion).Select(e=> e.EventData);
        }

        public int GetVersion(Guid aggregateId)
        {
            List<EventDescriptor> eventDescriptors;
            db.TryGetValue(aggregateId, out eventDescriptors);
            if (eventDescriptors == null)
                return 0;
            return eventDescriptors.Max(x => x.Version);
        }

        public void Save(Guid aggregateId, Event @event)
        {
            List<EventDescriptor> list;
            db.TryGetValue(aggregateId, out list);
            if(list == null)
            {
                list = new List<EventDescriptor>();
                db.Add(aggregateId, list);
            }
            var eventDescriptor = new EventDescriptor(@event.Id, @event, @event.Version);
            list.Add(eventDescriptor);
        }
    }
}

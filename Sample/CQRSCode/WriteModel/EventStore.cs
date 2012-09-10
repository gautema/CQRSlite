using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Contracts.Events;
using CQRSlite.Contracts.Messages;

namespace CQRSCode.WriteModel
{
    public class EventStore : IEventStore
    {
        private readonly Dictionary<Guid, List<Event>> db = new Dictionary<Guid, List<Event>>();

        public IEnumerable<Event> Get(Guid aggregateId, int fromVersion)
        {
            List<Event> events;
            db.TryGetValue(aggregateId, out events);
            return events.Where(x => x.Version > fromVersion);
        }

        public int GetVersion(Guid aggregateId)
        {
            List<Event> events;
            db.TryGetValue(aggregateId, out events);
            if (events == null)
                return 0;
            return events.Max(x => x.Version);
        }

        public void Save(Guid aggregateId, Event @event)
        {
            List<Event> list;
            db.TryGetValue(aggregateId, out list);
            if(list == null)
            {
                list = new List<Event>();
                db.Add(aggregateId, list);
            }
            list.Add(@event);
        }
    }
}

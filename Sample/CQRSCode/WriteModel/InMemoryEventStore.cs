using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Events;

namespace CQRSCode.WriteModel
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly Dictionary<Guid, List<Event>> _inMemoryDB = new Dictionary<Guid, List<Event>>();

        public IEnumerable<Event> Get(Guid aggregateId, int fromVersion)
        {
            List<Event> events;
            _inMemoryDB.TryGetValue(aggregateId, out events);
            return events.Where(x => x.Version > fromVersion);
        }

        public int GetVersion(Guid aggregateId)
        {
            List<Event> events;
            _inMemoryDB.TryGetValue(aggregateId, out events);
            if (events == null)
                return 0;
            return events.Max(x => x.Version);
        }

        public void Save(Guid aggregateId, Event @event)
        {
            List<Event> list;
            _inMemoryDB.TryGetValue(aggregateId, out list);
            if(list == null)
            {
                list = new List<Event>();
                _inMemoryDB.Add(aggregateId, list);
            }
            list.Add(@event);
        }
    }
}

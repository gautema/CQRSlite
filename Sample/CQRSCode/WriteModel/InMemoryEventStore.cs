using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Events;

namespace CQRSCode.WriteModel
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly Dictionary<Guid, List<IEvent>> _inMemoryDB = new Dictionary<Guid, List<IEvent>>();

        public IEnumerable<IEvent> Get(Guid aggregateId, int fromVersion)
        {
            List<IEvent> events;
            _inMemoryDB.TryGetValue(aggregateId, out events);
            return events.Where(x => x.Version > fromVersion);
        }

        public int GetVersion(Guid aggregateId)
        {
            List<IEvent> events;
            _inMemoryDB.TryGetValue(aggregateId, out events);
            if (events == null)
                return 0;
            return events.Max(x => x.Version);
        }

        public void Save(IEvent @event)
        {
            List<IEvent> list;
            _inMemoryDB.TryGetValue(@event.Id, out list);
            if(list == null)
            {
                list = new List<IEvent>();
                _inMemoryDB.Add(@event.Id, list);
            }
            list.Add(@event);
        }
    }
}

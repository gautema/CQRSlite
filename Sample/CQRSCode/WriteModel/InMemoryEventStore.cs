using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Events;

namespace CQRSCode.WriteModel
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly Dictionary<Guid, List<IEvent>> _inMemoryDB = new Dictionary<Guid, List<IEvent>>();

        public void Save(IEnumerable<IEvent> events)
        {
            foreach (var @event in events)
            {
                List<IEvent> list;
                _inMemoryDB.TryGetValue(@event.Id, out list);
                if (list == null)
                {
                    list = new List<IEvent>();
                    _inMemoryDB.Add(@event.Id, list);
                }
                list.Add(@event);
            }
        }

        public IEnumerable<IEvent> Get(Guid aggregateId, int fromVersion)
        {
            List<IEvent> events;
            _inMemoryDB.TryGetValue(aggregateId, out events);
            return events?.Where(x => x.Version > fromVersion) ?? new List<IEvent>();
        }
    }
}

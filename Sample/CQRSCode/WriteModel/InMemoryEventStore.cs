using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Events;

namespace CQRSCode.WriteModel
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly IEventPublisher _publisher;
        private readonly Dictionary<Guid, List<IEvent>> _inMemoryDb = new Dictionary<Guid, List<IEvent>>();

        public InMemoryEventStore(IEventPublisher publisher)
        {
            _publisher = publisher;
        }

        public void Save(IEnumerable<IEvent> events)
        {
            foreach (var @event in events)
            {
                List<IEvent> list;
                _inMemoryDb.TryGetValue(@event.Id, out list);
                if (list == null)
                {
                    list = new List<IEvent>();
                    _inMemoryDb.Add(@event.Id, list);
                }
                list.Add(@event);
                _publisher.Publish(@event);
            }
        }

        public IEnumerable<IEvent> Get(Guid aggregateId, int fromVersion)
        {
            List<IEvent> events;
            _inMemoryDb.TryGetValue(aggregateId, out events);
            return events?.Where(x => x.Version > fromVersion) ?? new List<IEvent>();
        }
    }
}

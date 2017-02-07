using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public void Save<T>(IEnumerable<IEvent> events)
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

        public Task SaveAsync<T>(IEnumerable<IEvent> events)
        {
            Save<T>(events);
            return Task.FromResult(0);
        }

        public IEnumerable<IEvent> Get<T>(Guid aggregateId, int fromVersion)
        {
            List<IEvent> events;
            _inMemoryDb.TryGetValue(aggregateId, out events);
            return events?.Where(x => x.Version > fromVersion) ?? new List<IEvent>();
        }

        public Task<IEnumerable<IEvent>> GetAsync<T>(Guid aggregateId, int fromVersion)
        {
            return Task.FromResult(Get<T>(aggregateId, fromVersion));
        }
    }
}

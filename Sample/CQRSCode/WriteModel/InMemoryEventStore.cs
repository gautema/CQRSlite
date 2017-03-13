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

        public async Task Save<T>(IEnumerable<IEvent> events)
        {
            foreach (var @event in events)
            {
                _inMemoryDb.TryGetValue(@event.Id, out var list);
                if (list == null)
                {
                    list = new List<IEvent>();
                    _inMemoryDb.Add(@event.Id, list);
                }
                list.Add(@event);
                await _publisher.Publish(@event);
            }
        }

        public Task<IEnumerable<IEvent>> Get<T>(Guid aggregateId, int fromVersion)
        {
            _inMemoryDb.TryGetValue(aggregateId, out var events);
            return Task.FromResult(events?.Where(x => x.Version > fromVersion) ?? new List<IEvent>());
        }
    }
}

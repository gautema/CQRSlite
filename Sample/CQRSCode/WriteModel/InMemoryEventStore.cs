using CQRSlite.Domain.Exception;
using CQRSlite.Events;

namespace CQRSCode.WriteModel;

// Stands in for a real event store. Like a real one, it must refuse an event whose version is
// already stored: that is what makes two concurrent writes to the same aggregate safe.
public class InMemoryEventStore : IEventStore
{
    private readonly IEventPublisher _publisher;
    private readonly Dictionary<Guid, List<IEvent>> _inMemoryDb = new Dictionary<Guid, List<IEvent>>();

    public InMemoryEventStore(IEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        var newEvents = events.ToArray();
        lock (_inMemoryDb)
        {
            // Check everything before storing anything, so a refused save stores nothing
            foreach (var aggregateEvents in newEvents.GroupBy(e => e.Id))
            {
                var storedCount = _inMemoryDb.TryGetValue(aggregateEvents.Key, out var stored) ? stored.Count : 0;
                if (aggregateEvents.First().Version != storedCount + 1)
                {
                    throw new ConcurrencyException(aggregateEvents.Key);
                }
            }

            foreach (var @event in newEvents)
            {
                if (!_inMemoryDb.TryGetValue(@event.Id, out var list))
                {
                    list = new List<IEvent>();
                    _inMemoryDb.Add(@event.Id, list);
                }
                list.Add(@event);
            }
        }

        // Publish only once the events are stored
        foreach (var @event in newEvents)
        {
            await _publisher.Publish(@event, cancellationToken);
        }
    }

    public Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default)
    {
        lock (_inMemoryDb)
        {
            _inMemoryDb.TryGetValue(aggregateId, out var events);
            IEnumerable<IEvent> result = events?.Where(x => x.Version > fromVersion).ToList() ?? new List<IEvent>();
            return Task.FromResult(result);
        }
    }
}

using CQRSlite.Domain;
using CQRSlite.Events;

namespace CQRSlite.Caching;

/// <summary>
/// Thread safe repository decorator that caches aggregates between saves.
/// A cached aggregate is lent to one caller at a time: Get takes it out of the cache,
/// and a successful Save puts it back. Callers never share an aggregate instance.
/// </summary>
public class CacheRepository : IRepository
{
    private readonly IRepository _repository;
    private readonly IEventStore _eventStore;
    private readonly ICache _cache;

    // One lock per aggregate id, shared by all instances, removed when no one holds or waits for it
    private static readonly Dictionary<Guid, AggregateLock> _locks = new();

    /// <summary>
    /// Initialize a new instance of CacheRepository
    /// </summary>
    /// <param name="repository">Repository that gets aggregate from event store</param>
    /// <param name="eventStore">EventStore where concurrency checking can be fetched from</param>
    /// <param name="cache">Implementation of the cache</param>
    public CacheRepository(IRepository repository, IEventStore eventStore, ICache cache)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task Save<T>(T aggregate, int? expectedVersion = null,
        CancellationToken cancellationToken = default) where T : AggregateRoot
    {
        var @lock = await AggregateLock.Acquire(aggregate.Id, cancellationToken).ConfigureAwait(false);
        try
        {
            await _repository.Save(aggregate, expectedVersion, cancellationToken).ConfigureAwait(false);
            if (aggregate.Id != default)
            {
                await _cache.Set(aggregate.Id, aggregate).ConfigureAwait(false);
            }
        }
        catch (Exception)
        {
            await _cache.Remove(aggregate.Id).ConfigureAwait(false);
            throw;
        }
        finally
        {
            @lock.Release();
        }
    }

    public async Task<T> Get<T>(Guid aggregateId, CancellationToken cancellationToken = default)
        where T : AggregateRoot
    {
        var @lock = await AggregateLock.Acquire(aggregateId, cancellationToken).ConfigureAwait(false);
        try
        {
            T? cached = null;
            if (await _cache.IsTracked(aggregateId).ConfigureAwait(false))
            {
                // Take it out of the cache so no one else gets this instance until it is saved again.
                // An aggregate that is never saved (e.g. its command failed) is never put back.
                cached = (T?) await _cache.Get(aggregateId).ConfigureAwait(false);
                await _cache.Remove(aggregateId).ConfigureAwait(false);
            }

            // The entry can be evicted between IsTracked and Get, so fall back to the repository on null.
            if (cached != null)
            {
                var events = (await _eventStore.Get(aggregateId, cached.Version, cancellationToken).ConfigureAwait(false)).ToArray();
                var firstEvent = events.FirstOrDefault();
                if (firstEvent == null || firstEvent.Version == cached.Version + 1)
                {
                    cached.LoadFromHistory(events);
                    return cached;
                }
            }

            return await _repository.Get<T>(aggregateId, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            @lock.Release();
        }
    }
    private sealed class AggregateLock
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly Guid _id;
        private int _references;

        private AggregateLock(Guid id) => _id = id;

        public static async Task<AggregateLock> Acquire(Guid id, CancellationToken cancellationToken)
        {
            AggregateLock? @lock;
            lock (_locks)
            {
                if (!_locks.TryGetValue(id, out @lock))
                {
                    @lock = new AggregateLock(id);
                    _locks.Add(id, @lock);
                }
                @lock._references++;
            }

            try
            {
                await @lock._semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                @lock.RemoveReference();
                throw;
            }
            return @lock;
        }

        public void Release()
        {
            _semaphore.Release();
            RemoveReference();
        }

        private void RemoveReference()
        {
            lock (_locks)
            {
                if (--_references == 0)
                {
                    _locks.Remove(_id);
                }
            }
        }
    }
}

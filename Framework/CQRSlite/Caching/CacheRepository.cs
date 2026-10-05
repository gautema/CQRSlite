using CQRSlite.Domain;
using CQRSlite.Events;

namespace CQRSlite.Caching;

/// <summary>
/// Thread safe repository decorator that can cache aggregates.
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
            if (aggregate.Id != default && !await _cache.IsTracked(aggregate.Id).ConfigureAwait(false))
            {
                await _cache.Set(aggregate.Id, aggregate).ConfigureAwait(false);
            }
            await _repository.Save(aggregate, expectedVersion, cancellationToken).ConfigureAwait(false);
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
            if (await _cache.IsTracked(aggregateId).ConfigureAwait(false))
            {
                // The entry can be evicted between IsTracked and Get, so fall back to the repository on null.
                var cached = (T?) await _cache.Get(aggregateId).ConfigureAwait(false);
                // Uncommitted changes mean an earlier user changed it and never saved, so don't hand those changes on
                if (cached != null && cached.GetUncommittedChanges().Length == 0)
                {
                    var events = (await _eventStore.Get(aggregateId, cached.Version, cancellationToken).ConfigureAwait(false)).ToArray();
                    var firstEvent = events.FirstOrDefault();
                    if (firstEvent == null || firstEvent.Version == cached.Version + 1)
                    {
                        cached.LoadFromHistory(events);
                        return cached;
                    }
                }
                await _cache.Remove(aggregateId).ConfigureAwait(false);
            }

            var aggregate = await _repository.Get<T>(aggregateId, cancellationToken).ConfigureAwait(false);
            await _cache.Set(aggregateId, aggregate).ConfigureAwait(false);
            return aggregate;
        }
        catch (Exception)
        {
            await _cache.Remove(aggregateId).ConfigureAwait(false);
            throw;
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

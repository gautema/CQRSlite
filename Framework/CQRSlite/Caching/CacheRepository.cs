using CQRSlite.Domain;
using CQRSlite.Events;
using System.Collections.Concurrent;

namespace CQRSlite.Caching;

/// <summary>
/// Thread safe repository decorator that can cache aggregates.
/// </summary>
public class CacheRepository : IRepository
{
    private readonly IRepository _repository;
    private readonly IEventStore _eventStore;
    private readonly ICache _cache;

    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();

    private static SemaphoreSlim CreateLock(Guid _) => new(1, 1);

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

        _cache.RegisterEvictionCallback(key => _locks.TryRemove(key, out var _));
    }

    public async Task Save<T>(T aggregate, int? expectedVersion = null,
        CancellationToken cancellationToken = default) where T : AggregateRoot
    {
        var @lock = _locks.GetOrAdd(aggregate.Id, CreateLock);
        await @lock.WaitAsync(cancellationToken).ConfigureAwait(false);
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
        var @lock = _locks.GetOrAdd(aggregateId, CreateLock);
        await @lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (await _cache.IsTracked(aggregateId).ConfigureAwait(false))
            {
                // The entry can be evicted between IsTracked and Get, so fall back to the repository on null.
                var cached = (T?) await _cache.Get(aggregateId).ConfigureAwait(false);
                if (cached != null)
                {
                    var events = await _eventStore.Get(aggregateId, cached.Version, cancellationToken).ConfigureAwait(false);
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
}
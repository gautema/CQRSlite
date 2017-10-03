using CQRSlite.Domain;
using CQRSlite.Events;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSlite.Caching
{
    /// <summary>
    /// Thread safe repository decorator that can cache aggregates.
    /// </summary>
    public class CacheRepository : IRepository
    {
        private readonly IRepository _repository;
        private readonly IEventStore _eventStore;
        private readonly ICache _cache;

        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks =
            new ConcurrentDictionary<Guid, SemaphoreSlim>();

        private static SemaphoreSlim CreateLock(Guid _) => new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initialize a new instance of CacheRepository
        /// </summary>
        /// <param name="repository">Reposiory that gets aggregate from event store</param>
        /// <param name="eventStore">Eventstore where concurrency checking can be fetched from</param>
        /// <param name="cache">Implementation of the cache</param>
        public CacheRepository(IRepository repository, IEventStore eventStore, ICache cache)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            _cache.RegisterEvictionCallback(key => _locks.TryRemove(key, out var _));
        }

        public async Task Save<T>(T aggregate, int? expectedVersion = null,
            CancellationToken cancellationToken = default(CancellationToken)) where T : AggregateRoot
        {
            var @lock = _locks.GetOrAdd(aggregate.Id, CreateLock);
            await @lock.WaitAsync(cancellationToken);
            try
            {
                if (aggregate.Id != Guid.Empty && !await _cache.IsTracked(aggregate.Id))
                {
                    await _cache.Set(aggregate.Id, aggregate);
                }
                await _repository.Save(aggregate, expectedVersion, cancellationToken);
            }
            catch (Exception)
            {
                await _cache.Remove(aggregate.Id);
                throw;
            }
            finally
            {
                @lock.Release();
            }
        }

        public async Task<T> Get<T>(Guid aggregateId, CancellationToken cancellationToken = default(CancellationToken))
            where T : AggregateRoot
        {
            var @lock = _locks.GetOrAdd(aggregateId, CreateLock);
            await @lock.WaitAsync(cancellationToken);
            try
            {
                T aggregate;
                if (await _cache.IsTracked(aggregateId))
                {
                    aggregate = (T) await _cache.Get(aggregateId);
                    var events = await _eventStore.Get(aggregateId, aggregate.Version, cancellationToken);
                    if (events.Any() && events.First().Version != aggregate.Version + 1)
                    {
                        await _cache.Remove(aggregateId);
                    }
                    else
                    {
                        aggregate.LoadFromHistory(events);
                        return aggregate;
                    }
                }

                aggregate = await _repository.Get<T>(aggregateId, cancellationToken);
                await _cache.Set(aggregateId, aggregate);
                return aggregate;
            }
            catch (Exception)
            {
                await _cache.Remove(aggregateId);
                throw;
            }
            finally
            {
                @lock.Release();
            }
        }
    }
}
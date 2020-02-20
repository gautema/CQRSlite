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
                T aggregate;
                if (await _cache.IsTracked(aggregateId).ConfigureAwait(false))
                {
                    aggregate = (T) await _cache.Get(aggregateId).ConfigureAwait(false);
                    var events = await _eventStore.Get(aggregateId, aggregate.Version, cancellationToken).ConfigureAwait(false);
                    var firstEvent = events.FirstOrDefault();
                    if (firstEvent != null && firstEvent.Version != aggregate.Version + 1)
                    {
                        await _cache.Remove(aggregateId).ConfigureAwait(false);
                    }
                    else
                    {
                        aggregate.LoadFromHistory(events);
                        return aggregate;
                    }
                }

                aggregate = await _repository.Get<T>(aggregateId, cancellationToken).ConfigureAwait(false);
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
}
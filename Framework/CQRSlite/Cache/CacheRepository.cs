using CQRSlite.Domain;
using CQRSlite.Events;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSlite.Cache
{
    public class CacheRepository : IRepository
    {
        private readonly IRepository _repository;
        private readonly IEventStore _eventStore;
        private readonly ICache _cache;
        private static readonly ConcurrentDictionary<Guid, ManualResetEvent> _locks = new ConcurrentDictionary<Guid, ManualResetEvent>();

        public CacheRepository(IRepository repository, IEventStore eventStore, ICache cache)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            _cache.RegisterEvictionCallback(key =>
                     {
                         _locks.TryRemove(key, out var o);
                         o?.Set();
                     });

        }

        public Task Save<T>(T aggregate, int? expectedVersion = null) where T : AggregateRoot
        {
            try
            {
                lock (_locks.GetOrAdd(aggregate.Id, _ => new ManualResetEvent(false)))
                {
                    if (aggregate.Id != Guid.Empty && !_cache.IsTracked(aggregate.Id))
                    {
                        _cache.Set(aggregate.Id, aggregate);
                    }
                    return _repository.Save(aggregate, expectedVersion);
                }
            }
            catch (Exception)
            {
                lock (_locks.GetOrAdd(aggregate.Id, _ => new ManualResetEvent(false)))
                {
                    _cache.Remove(aggregate.Id);
                }
                throw;
            }
        }

        public Task<T> Get<T>(Guid aggregateId) where T : AggregateRoot
        {
            try
            {
                lock (_locks.GetOrAdd(aggregateId, _ => new ManualResetEvent(false)))
                {
                    T aggregate;
                    if (_cache.IsTracked(aggregateId))
                    {
                        aggregate = (T)_cache.Get(aggregateId);
                        var events = _eventStore.Get(aggregateId, aggregate.Version).Result;
                        if (events.Any() && events.First().Version != aggregate.Version + 1)
                        {
                            _cache.Remove(aggregateId);
                        }
                        else
                        {
                            aggregate.LoadFromHistory(events);
                            return Task.FromResult(aggregate);
                        }
                    }

                    aggregate = _repository.Get<T>(aggregateId).Result;
                    _cache.Set(aggregateId, aggregate);
                    return Task.FromResult(aggregate);
                }
            }
            catch (Exception)
            {
                lock (_locks.GetOrAdd(aggregateId, _ => new ManualResetEvent(false)))
                {
                    _cache.Remove(aggregateId);
                }
                throw;
            }
        }
    }
}
using CQRSlite.Domain;
using CQRSlite.Events;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

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
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }
            if (eventStore == null)
            {
                throw new ArgumentNullException(nameof(eventStore));
            }
            if (cache == null)
            {
                throw new ArgumentNullException(nameof(cache));
            }

            _repository = repository;
            _eventStore = eventStore;
            _cache = cache;
            _cache.RegisterEvictionCallback(key =>
                     {
                         ManualResetEvent o;
                         _locks.TryRemove(key, out o);
                         o.Set();
                     });

        }

        public void Save<T>(T aggregate, int? expectedVersion = null) where T : AggregateRoot
        {
            try
            {
                lock (_locks.GetOrAdd(aggregate.Id, _ => new ManualResetEvent(false)))
                {
                    if (aggregate.Id != Guid.Empty && !_cache.IsTracked(aggregate.Id))
                    {
                        _cache.Set(aggregate.Id, aggregate);
                    }
                    _repository.Save(aggregate, expectedVersion);
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

        public T Get<T>(Guid aggregateId) where T : AggregateRoot
        {
            try
            {
                lock (_locks.GetOrAdd(aggregateId, _ => new ManualResetEvent(false)))
                {
                    T aggregate;
                    if (_cache.IsTracked(aggregateId))
                    {
                        aggregate = (T)_cache.Get(aggregateId);
                        var events = _eventStore.Get<T>(aggregateId, aggregate.Version);
                        if (events.Any() && events.First().Version != aggregate.Version + 1)
                        {
                            _cache.Remove(aggregateId);
                        }
                        else
                        {
                            aggregate.LoadFromHistory(events);
                            return aggregate;
                        }
                    }

                    aggregate = _repository.Get<T>(aggregateId);
                    _cache.Set(aggregateId, aggregate);
                    return aggregate;
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
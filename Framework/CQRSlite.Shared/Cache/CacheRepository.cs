using CQRSlite.Domain;
using CQRSlite.Events;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace CQRSlite.Cache
{

#if NETSTANDARD1_3
    using Microsoft.Extensions.Caching.Memory;
#elif NETSTANDARD1_1
    using Provision.Providers.PortableMemoryCache;
    using Provision.Providers.PortableMemoryCache.Mono;
#else
    using System.Runtime.Caching; 
#endif

    public class CacheRepository : IRepository
    {
        private readonly IRepository _repository;
        private readonly IEventStore _eventStore;
#if NETSTANDARD1_1
        private readonly PortableMemoryCacheHandler _cache;
#else
        private readonly MemoryCache _cache;
#endif
        private static readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

#if !NETSTANDARD1_3 && !NETSTANDARD1_1
        private readonly Func<CacheItemPolicy> _policyFactory;
#endif

        public CacheRepository(IRepository repository, IEventStore eventStore)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }
            if (eventStore == null)
            {
                throw new ArgumentNullException(nameof(eventStore));
            }

            _repository = repository;
            _eventStore = eventStore;
#if NETSTANDARD1_3
            _cache = new MemoryCache(new MemoryCacheOptions() {ExpirationScanFrequency= TimeSpan.FromMinutes(15) });
#elif NETSTANDARD1_1
            var config = new Provision.Providers.PortableMemoryCache.PortableMemoryCacheHandlerConfiguration(TimeSpan.FromMinutes(15));
            _cache = new Provision.Providers.PortableMemoryCache.PortableMemoryCacheHandler(config);

#else
            _cache = MemoryCache.Default;
            _policyFactory = () => new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromMinutes(15),
                RemovedCallback = x =>
                {
                    object o;
                    _locks.TryRemove(x.CacheItem.Key, out o);
                }
            };

#endif
        }

        public void Save<T>(T aggregate, int? expectedVersion = null) where T : AggregateRoot
        {
            var idstring = aggregate.Id.ToString();
            try
            {
                lock (_locks.GetOrAdd(idstring, _ => new object()))
                {
                    if (aggregate.Id != Guid.Empty && !IsTracked(aggregate.Id))
                    {
#if NETSTANDARD1_3
                        _cache.Set(idstring, aggregate);
#elif NETSTANDARD1_1
                        _cache.AddOrUpdate(idstring, aggregate);

#else
                        _cache.Add(idstring, aggregate, _policyFactory.Invoke());

#endif
                    }
                    _repository.Save(aggregate, expectedVersion);
                }
            }
            catch (Exception)
            {
                lock (_locks.GetOrAdd(idstring, _ => new object()))
                {
#if NETSTANDARD1_1
                    _cache.RemoveByKey(idstring);
#else
                    _cache.Remove(idstring);
#endif
                    object o;
                    _locks.TryRemove(idstring, out o);
                }
                throw;
            }
        }

        public T Get<T>(Guid aggregateId) where T : AggregateRoot
        {
            var idstring = aggregateId.ToString();
            try
            {
                lock (_locks.GetOrAdd(idstring, _ => new object()))
                {
                    T aggregate;
                    if (IsTracked(aggregateId))
                    {
#if NETSTANDARD1_1
                        aggregate = _cache.GetValue<T>(idstring).Result;
#else
                        aggregate = (T)_cache.Get(idstring);
#endif
                        var events = _eventStore.Get<T>(aggregateId, aggregate.Version);
                        if (events.Any() && events.First().Version != aggregate.Version + 1)
                        {
#if NETSTANDARD1_1
                            _cache.RemoveByKey(idstring);
#else
                    _cache.Remove(idstring);
#endif
                            object o;
                            _locks.TryRemove(idstring, out o);
                        }
                        else
                        {
                            aggregate.LoadFromHistory(events);
                            return aggregate;
                        }
                    }

                    aggregate = _repository.Get<T>(aggregateId);
#if NETSTANDARD1_3
                    _cache.Set(aggregateId.ToString(), aggregate);
#elif NETSTANDARD1_1
                    _cache.AddOrUpdate(aggregateId.ToString(), aggregate);
#else
                    _cache.Add(aggregateId.ToString(), aggregate, _policyFactory.Invoke());
#endif
                    return aggregate;
                }
            }
            catch (Exception)
            {
                lock (_locks.GetOrAdd(idstring, _ => new object()))
                {
#if NETSTANDARD1_1
                    _cache.RemoveByKey(idstring);
#else
                    _cache.Remove(idstring);
#endif
                    object o;
                    _locks.TryRemove(idstring, out o);
                }
                throw;
            }
        }

        private bool IsTracked(Guid id)
        {
#if NETSTANDARD1_3
            object result;
            return _cache.TryGetValue(id.ToString(), out result);
#elif NETSTANDARD1_1
            return _cache.Contains(id.ToString()).Result;

#else
            return _cache.Contains(id.ToString());
#endif
        }
    }
}
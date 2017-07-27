using System;
using CQRSlite.Domain;
#if NET452
using System.Runtime.Caching;
#else
using Microsoft.Extensions.Caching.Memory;
#endif

namespace CQRSlite.Cache
{
    public class MemoryCache : ICache
    {
#if NET452
        private readonly System.Runtime.Caching.MemoryCache _cache;
        private Func<CacheItemPolicy> _policyFactory;
#else
        private readonly MemoryCacheEntryOptions _cacheOptions;
        private readonly IMemoryCache _cache;
#endif

        public MemoryCache()
        {

#if NET452
            _cache = System.Runtime.Caching.MemoryCache.Default;
            _policyFactory = () => new CacheItemPolicy {
                SlidingExpiration = TimeSpan.FromMinutes(15)
            };
#else
            _cacheOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(15)
            };
            _cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new MemoryCacheOptions());
#endif

        }

        public bool IsTracked(Guid id)
        {
#if NET452
            return _cache.Contains(id.ToString());
#else
            return _cache.TryGetValue(id, out var o) && o != null;
#endif
        }

        public void Set(Guid id, AggregateRoot aggregate)
        {
#if NET452
            _cache.Add(id.ToString(), aggregate, _policyFactory.Invoke());
#else
            _cache.Set(id, aggregate, _cacheOptions);
#endif
        }

        public AggregateRoot Get(Guid id)
        {
#if NET452
            return (AggregateRoot)_cache.Get(id.ToString());
#else
            return (AggregateRoot) _cache.Get(id);
#endif
        }

        public void Remove(Guid id)
        {
#if NET452
            _cache.Remove(id.ToString());
#else
            _cache.Remove(id);
#endif
        }

        public void RegisterEvictionCallback(Action<Guid> action)
        {
#if NET452
            _policyFactory = () => new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromMinutes(15),
                RemovedCallback = x =>
                {
                    action.Invoke(Guid.Parse(x.CacheItem.Key));
                }
            };
#else
            _cacheOptions.RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                action.Invoke((Guid) key);
            });
#endif
        }
    }
}

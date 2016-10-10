using System;
using CQRSlite.Domain;
#if NET451
using System.Runtime.Caching;
#else
using Microsoft.Extensions.Caching.Memory;
#endif

namespace CQRSlite.Cache
{
    public class MemoryCache : ICache
    {
#if NET451
        private readonly System.Runtime.Caching.MemoryCache _cache;
        private Func<CacheItemPolicy> _policyFactory;
#else
        private readonly MemoryCacheEntryOptions _cacheOptions;
        private readonly IMemoryCache _cache;
#endif

        public MemoryCache()
        {

#if NET451
            _cache = System.Runtime.Caching.MemoryCache.Default;
            _policyFactory = () => new CacheItemPolicy();
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
            object o;
#if NET451
            return _cache.Contains(id.ToString());
#else
            return _cache.TryGetValue(id, out o);
#endif
        }

        public void Set(Guid id, AggregateRoot aggregate)
        {
#if NET451
            _cache.Add(id.ToString(), aggregate, _policyFactory.Invoke());
#else
            _cache.Set(id, aggregate, _cacheOptions);
#endif
        }

        public AggregateRoot Get(Guid id)
        {
#if NET451
            return (AggregateRoot)_cache.Get(id.ToString());
#else
            return (AggregateRoot) _cache.Get(id);
#endif
        }

        public void Remove(Guid id)
        {
#if NET451
            _cache.Remove(id.ToString());
#else
            _cache.Remove(id);
#endif
        }

        public void RegisterEvictionCallback(Action<Guid> action)
        {
#if NET451
            _policyFactory = () => new CacheItemPolicy
            {
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

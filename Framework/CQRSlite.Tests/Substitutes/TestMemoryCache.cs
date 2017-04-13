using System;
using System.Collections.Generic;
using CQRSlite.Cache;
using CQRSlite.Domain;

namespace CQRSlite.Tests.Substitutes
{
    public class TestMemoryCache : ICache
    {
        private readonly Dictionary<Guid, AggregateRoot> _cache = new Dictionary<Guid, AggregateRoot>();

        private Action<Guid> _evictionCallback;

        public AggregateRoot Get(Guid id)
        {
            return _cache[id];
        }

        public bool IsTracked(Guid id)
        {
            return _cache.ContainsKey(id);
        }

        public void RegisterEvictionCallback(Action<Guid> action)
        {
            _evictionCallback = action;
        }

        public void Remove(Guid id)
        {
            _cache.Remove(id);
            _evictionCallback(id);
        }

        public void Set(Guid id, AggregateRoot aggregate)
        {
            _cache[id] = aggregate;
        }
    }
}
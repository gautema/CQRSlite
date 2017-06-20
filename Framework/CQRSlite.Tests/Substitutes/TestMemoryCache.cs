using System;
using System.Collections.Generic;
using CQRSlite.Cache;
using CQRSlite.Domain;

namespace CQRSlite.Tests.Substitutes
{
    public class TestMemoryCache : ICache
    {
        private readonly Dictionary<IIdentity, AggregateRoot> _cache = new Dictionary<IIdentity, AggregateRoot>();

        private Action<IIdentity> _evictionCallback;

        public AggregateRoot Get(IIdentity id)
        {
            return _cache[id];
        }

        public bool IsTracked(IIdentity id)
        {
            return _cache.ContainsKey(id);
        }

        public void RegisterEvictionCallback(Action<IIdentity> action)
        {
            _evictionCallback = action;
        }

        public void Remove(IIdentity id)
        {
            _cache.Remove(id);
            _evictionCallback(id);
        }

        public void Set(IIdentity id, AggregateRoot aggregate)
        {
            _cache[id] = aggregate;
        }
    }
}
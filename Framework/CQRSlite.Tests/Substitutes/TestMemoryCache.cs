using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CQRSlite.Caching;
using CQRSlite.Domain;

namespace CQRSlite.Tests.Substitutes
{
    public class TestMemoryCache : ICache
    {
        private readonly Dictionary<Guid, AggregateRoot> _cache = new Dictionary<Guid, AggregateRoot>();

        private Action<Guid> _evictionCallback;

        public Task<AggregateRoot> Get(Guid id)
        {
            return Task.FromResult(_cache[id]);
        }

        public Task<bool> IsTracked(Guid id)
        {
            return Task.FromResult(_cache.ContainsKey(id));
        }

        public void RegisterEvictionCallback(Action<Guid> action)
        {
            _evictionCallback = action;
        }

        public Task Remove(Guid id)
        {
            _cache.Remove(id);
            _evictionCallback(id);
            return Task.CompletedTask;
        }

        public Task Set(Guid id, AggregateRoot aggregate)
        {
            _cache[id] = aggregate;
            return Task.CompletedTask;
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Reflection;
using CQRSlite.Cache;
using CQRSlite.Tests.Substitutes;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace CQRSlite.Tests.Cache
{
    public class When_evicting_cache_entry
    {
        private CacheRepository _rep;
        private TestAggregate _aggregate;
        private IMemoryCache _memoryCache;

        public When_evicting_cache_entry()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _rep = new CacheRepository(new TestRepository(), new TestEventStore(), _memoryCache);
            _aggregate = _rep.Get<TestAggregate>(Guid.NewGuid());
            _memoryCache.Remove(_aggregate.Id.ToString());
        }

        [Fact]
        public void Should_remove_lock()
        {
            var field = _rep.GetType().GetField("_locks", BindingFlags.Static | BindingFlags.NonPublic);
            var locks = (ConcurrentDictionary<string, object>)field.GetValue(_rep);
            Assert.Empty(locks);
        }

        [Fact]
        public void Should_get_new_aggregate_next_get()
        {
            var aggregate = _rep.Get<TestAggregate>(_aggregate.Id);
            Assert.NotEqual(_aggregate, aggregate);
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
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
        private MemoryCache _memoryCache;
        private ConcurrentDictionary<string, object> _locks;

        public When_evicting_cache_entry()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _rep = new CacheRepository(new TestRepository(), new TestEventStore(), _memoryCache);
            _aggregate = _rep.Get<TestAggregate>(Guid.NewGuid());
            object resetEvent;
            var field = _rep.GetType().GetField("_locks", BindingFlags.Static | BindingFlags.NonPublic);
            _locks = (ConcurrentDictionary<string, object>)field.GetValue(_rep);
            _locks.TryGetValue(_aggregate.Id.ToString(), out resetEvent);
            _memoryCache.Remove(_aggregate.Id.ToString());
            ((ManualResetEvent)resetEvent).WaitOne(500);
        }

        [Fact]
        public void Should_remove_lock()
        {
            object val;
            _locks.TryGetValue(_aggregate.Id.ToString(), out val);
            Assert.Null(val);
        }

        [Fact]
        public void Should_get_new_aggregate_next_get()
        {
            _memoryCache.Remove(_aggregate.Id.ToString());

            var aggregate = _rep.Get<TestAggregate>(_aggregate.Id);
            Assert.NotEqual(_aggregate, aggregate);
        }
    }
}

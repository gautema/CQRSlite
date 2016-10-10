using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using CQRSlite.Cache;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Cache
{
    public class When_evicting_cache_entry
    {
        private CacheRepository _rep;
        private TestAggregate _aggregate;
        private MemoryCache _memoryCache;
        private ConcurrentDictionary<Guid, ManualResetEvent> _locks;

        public When_evicting_cache_entry()
        {
            _memoryCache = new MemoryCache();
            _rep = new CacheRepository(new TestRepository(), new TestEventStore(), _memoryCache);
            _aggregate = _rep.Get<TestAggregate>(Guid.NewGuid());
            ManualResetEvent resetEvent;
            var field = _rep.GetType().GetField("_locks", BindingFlags.Static | BindingFlags.NonPublic);
            _locks = (ConcurrentDictionary<Guid, ManualResetEvent>)field.GetValue(_rep);
            _locks.TryGetValue(_aggregate.Id, out resetEvent);
            _memoryCache.Remove(_aggregate.Id);
            resetEvent.WaitOne(500);
        }

        [Fact]
        public void Should_remove_lock()
        {
            ManualResetEvent val;
            _locks.TryGetValue(_aggregate.Id, out val);
            Assert.Null(val);
        }

        [Fact]
        public void Should_get_new_aggregate_next_get()
        {
            _memoryCache.Remove(_aggregate.Id);

            var aggregate = _rep.Get<TestAggregate>(_aggregate.Id);
            Assert.NotEqual(_aggregate, aggregate);
        }
    }
}

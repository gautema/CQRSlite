using CQRSlite.Cache;
using CQRSlite.Tests.Substitutes;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CQRSlite.Tests.Cache
{
    public class When_getting_wrong_events_from_event_source_async
    {
        private CacheRepository _rep;
        private TestAggregate _aggregate;
        private ICache _memoryCache;

        public When_getting_wrong_events_from_event_source_async()
        {
            _memoryCache = new MemoryCache();
            _rep = new CacheRepository(new TestRepository(), new TestEventStoreWithBugs(), _memoryCache);
            _aggregate = _rep.GetAsync<TestAggregate>(Guid.NewGuid()).Result;
        }

        [Fact]
        public async Task Should_evict_old_object_from_cache()
        {
            await _rep.GetAsync<TestAggregate>(_aggregate.Id);
            var aggregate = _memoryCache.Get(_aggregate.Id);
            Assert.NotEqual(_aggregate, aggregate);
        }

        [Fact]
        public async Task Should_get_events_from_start()
        {
            var aggregate = await _rep.GetAsync<TestAggregate>(_aggregate.Id);
            Assert.Equal(1, aggregate.Version);
        }
    }
}

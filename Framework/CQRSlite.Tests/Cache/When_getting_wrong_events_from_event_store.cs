using System;
using CQRSlite.Cache;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Cache
{
    public class When_getting_earlier_than_expected_events_from_event_store
    {
        private CacheRepository _rep;
        private TestAggregate _aggregate;
        private ICache _memoryCache;

        public When_getting_earlier_than_expected_events_from_event_store()
        {
            _memoryCache = new MemoryCache();
            _rep = new CacheRepository(new TestRepository(), new TestEventStoreWithBugs(), _memoryCache);
            _aggregate = _rep.Get<TestAggregate>(Guid.NewGuid());
        }

        [Fact]
        public void Should_evict_old_object_from_cache()
        {
            _rep.Get<TestAggregate>(_aggregate.Id);
            var aggregate = _memoryCache.Get(_aggregate.Id);
            Assert.NotEqual(_aggregate, aggregate);
        }

        [Fact]
        public void Should_get_events_from_start()
        {
            var aggregate =_rep.Get<TestAggregate>(_aggregate.Id);
            Assert.Equal(1, aggregate.Version);
        }
    }
}
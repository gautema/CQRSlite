using System;
using CQRSlite.Cache;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Cache
{
    public class When_getting_aggregate
    {
        private CacheRepository _rep;
        private TestAggregate _aggregate;
        private ICache _memoryCache;

        public When_getting_aggregate()
        {
            _memoryCache = new MemoryCache();
            _rep = new CacheRepository(new TestRepository(), new TestEventStore(), _memoryCache);
            _aggregate = _rep.Get<TestAggregate>(Guid.NewGuid());
        }

        [Fact]
        public void Should_get_aggregate()
        {
            Assert.NotNull(_aggregate);
        }

        [Fact]
        public void Should_get_same_aggregate_on_second_try()
        {
            var aggregate =_rep.Get<TestAggregate>(_aggregate.Id);
            Assert.Equal(_aggregate, aggregate);
        }

        [Fact]
        public void Should_update_if_version_changed_in_event_store()
        {
            var aggregate = _rep.Get<TestAggregate>(_aggregate.Id);
            Assert.Equal(3, _aggregate.Version);
        }

        [Fact]
        public void Should_get_same_aggregate_from_different_cache_repository()
        {
            var rep = new CacheRepository(new TestRepository(), new TestInMemoryEventStore(), _memoryCache);
            var aggregate = rep.Get<TestAggregate>(_aggregate.Id);
            Assert.Equal(_aggregate.DidSomethingCount, aggregate.DidSomethingCount);
            Assert.Equal(_aggregate.Id, aggregate.Id);
            Assert.Equal(_aggregate.Version, aggregate.Version);
        }
    }
}
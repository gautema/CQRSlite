using System;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Caching;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Caching
{
    public class When_getting_aggregate
    {
        private readonly CacheRepository _rep;
        private readonly TestAggregate _aggregate;
        private readonly ICache _cache;
        private readonly TestEventStore _testEventStore;

        public When_getting_aggregate()
        {
            _testEventStore = new TestEventStore();
            _cache = new MemoryCache();
            _rep = new CacheRepository(new TestRepository(), _testEventStore, _cache);
            _aggregate = _rep.Get<TestAggregate>(Guid.NewGuid()).Result;
        }

        [Fact]
        public void Should_get_aggregate()
        {
            Assert.NotNull(_aggregate);
        }

        [Fact]
        public async Task Should_get_same_aggregate_on_second_try()
        {
            var aggregate = await _rep.Get<TestAggregate>(_aggregate.Id);
            Assert.Equal(_aggregate, aggregate);
        }

        [Fact]
        public async Task Should_update_if_version_changed_in_event_store()
        {
            var aggregate = await _rep.Get<TestAggregate>(_aggregate.Id);
            Assert.Equal(3, aggregate.Version);
        }

        [Fact]
        public async Task Should_get_same_aggregate_from_different_cache_repository()
        {
            var rep = new CacheRepository(new TestRepository(), new TestInMemoryEventStore(), _cache);
            var aggregate = await rep.Get<TestAggregate>(_aggregate.Id);
            Assert.Equal(_aggregate.DidSomethingCount, aggregate.DidSomethingCount);
            Assert.Equal(_aggregate.Id, aggregate.Id);
            Assert.Equal(_aggregate.Version, aggregate.Version);
        }

        [Fact]
        public async Task Should_forward_cancellation_token()
        {
            var token = new CancellationToken();
            await _rep.Get<TestAggregate>(_aggregate.Id, token);
            Assert.Equal(token, _testEventStore.Token);
        }
    }
}
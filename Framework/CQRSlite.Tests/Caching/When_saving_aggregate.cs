using System;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Caching;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Caching
{
    public class When_saving_aggregate
    {
        private CacheRepository _rep;
        private TestAggregate _aggregate;
        private TestRepository _testRep;
        private TestInMemoryEventStore _testEventStore;

        public When_saving_aggregate()
        {
            _testRep = new TestRepository();
            _testEventStore = new TestInMemoryEventStore();
            _rep = new CacheRepository(_testRep, _testEventStore, new MemoryCache());
            _aggregate = _testRep.Get<TestAggregate>(Guid.NewGuid()).Result;
            _aggregate.DoSomething();
            _rep.Save(_aggregate, -1).Wait();
        }

        [Fact]
        public async Task Should_get_same_aggregate_on_get()
        {
            var aggregate = await _rep.Get<TestAggregate>(_aggregate.Id);
            Assert.Equal(_aggregate, aggregate);
        }

        [Fact]
        public void Should_save_to_repository()
        {
            Assert.Equal(_aggregate, _testRep.Saved);
        }

        [Fact]
        public async Task Should_not_cache_empty_id()
        {
            var aggregate = new TestAggregate(Guid.Empty);
            await _rep.Save(aggregate);
            Assert.NotEqual(aggregate, await _rep.Get<TestAggregate>(Guid.Empty));
        }

        [Fact]
        public async Task Should_forward_cancellation_token()
        {
            var token = new CancellationToken();
            var aggregate = new TestAggregate(Guid.Empty);
            await _rep.Save(aggregate, cancellationToken: token);
            Assert.Equal(token, _testEventStore.Token);
        }
    }
}
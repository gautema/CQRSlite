using CQRSlite.Cache;
using CQRSlite.Tests.Substitutes;
using System;
using Xunit;

namespace CQRSlite.Tests.Cache
{
    public class When_saving_fails_async
    {
        private CacheRepository _rep;
        private TestAggregate _aggregate;
        private TestRepository _testRep;
        private ICache _memoryCache;

        public When_saving_fails_async()
        {
            _memoryCache = new MemoryCache();
            _testRep = new TestRepository();
            _rep = new CacheRepository(_testRep, new TestInMemoryEventStore(), _memoryCache);
            _aggregate = _testRep.GetAsync<TestAggregate>(Guid.NewGuid()).Result;
            _aggregate.DoSomething();
            try
            {
                _rep.SaveAsync(_aggregate, 100).Wait();
            }
            catch (Exception) { }
        }

        [Fact]
        public void Should_evict_old_object_from_cache()
        {
            var aggregate = _memoryCache.Get(_aggregate.Id);
            Assert.Null(aggregate);
        }
    }
}

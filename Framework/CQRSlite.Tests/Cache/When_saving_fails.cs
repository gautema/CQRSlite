using System;
using CQRSlite.Cache;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Cache
{
    public class When_saving_fails
    {
        private CacheRepository _rep;
        private TestAggregate _aggregate;
        private TestRepository _testRep;
        private ICache _memoryCache;

        public When_saving_fails()
        {
            _memoryCache = new MemoryCache();
            _testRep = new TestRepository();
            _rep = new CacheRepository(_testRep, new TestInMemoryEventStore(), _memoryCache);
            _aggregate = _testRep.Get<TestAggregate>(Guid.NewGuid());
            _aggregate.DoSomething();
            try
            {
                _rep.Save(_aggregate, 100);
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
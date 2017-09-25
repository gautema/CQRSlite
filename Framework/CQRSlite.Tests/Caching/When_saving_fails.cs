using System;
using System.Threading.Tasks;
using CQRSlite.Caching;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Caching
{
    public class When_saving_fails
    {
        private CacheRepository _rep;
        private TestAggregate _aggregate;
        private TestRepository _testRep;
        private ICache _cache;

        public When_saving_fails()
        {
            _cache = new MemoryCache();
            _testRep = new TestRepository();
            _rep = new CacheRepository(_testRep, new TestInMemoryEventStore(), _cache);
            _aggregate = _testRep.Get<TestAggregate>(Guid.NewGuid()).Result;
            _aggregate.DoSomething();
            _testRep.Throw = true;
            try
            {
                _rep.Save(_aggregate).Wait();
            }
            catch (Exception) { }
        }

        [Fact]
        public async Task Should_evict_old_object_from_cache()
        {
            var aggregate = await _cache.Get(_aggregate.Id);
            Assert.Null(aggregate);
        }
    }
}
using System;
using System.Threading.Tasks;
using CQRSlite.Caching;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Caching
{
    public class When_saving_fails
    {
        private readonly TestAggregate _aggregate;
        private readonly ICache _cache;

        public When_saving_fails()
        {
            _cache = new MemoryCache();
            var testRep = new TestRepository();
            var rep = new CacheRepository(testRep, new TestInMemoryEventStore(), _cache);
            _aggregate = testRep.Get<TestAggregate>(Guid.NewGuid()).Result;
            _aggregate.DoSomething();
            testRep.Throw = true;
            try
            {
                rep.Save(_aggregate).Wait();
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
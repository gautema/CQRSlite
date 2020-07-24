using System;
using System.Linq;
using System.Threading.Tasks;
using CQRSlite.Caching;
using CQRSlite.Domain;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Caching
{
    public class When_creating_and_using_repositories_in_parallel
    {
        private readonly TestInMemoryEventStore _testStore;

        private readonly ICache _cache;

        private Exception _exception;

        public When_creating_and_using_repositories_in_parallel()
        {
            _testStore = new TestInMemoryEventStore();
            _cache = new CQRSlite.Caching.MemoryCache();

            int numberOfAggregates = 10;
            int numberOfIterations = 200;

            Task[] tasks = Enumerable.Range(0, numberOfAggregates).Select(_ => Task.Run(async () =>
            {
                var aggregateId = Guid.NewGuid();
                for (int iteration = 0; iteration < numberOfIterations; iteration++)
                {
                    var cacheRepository = new CacheRepository(new Repository(_testStore), _testStore, _cache);
                    await cacheRepository.Save(new TestAggregate(aggregateId));
                }
            })).ToArray();

            _exception = Record.Exception(() => Task.WaitAll(tasks));
        }

        [Fact]
        public void Should_not_cause_concurrency_issues()
        {
            Assert.Null(_exception);
        }
    }
}
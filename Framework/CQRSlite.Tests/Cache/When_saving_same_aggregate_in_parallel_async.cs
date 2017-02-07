using System;
using System.Linq;
using System.Threading.Tasks;
using CQRSlite.Cache;
using CQRSlite.Domain;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Cache
{
    public class When_saving_same_aggregate_in_parallel_async
    {
        private CacheRepository _rep1;
        private CacheRepository _rep2;
        private TestAggregate _aggregate;
        private TestInMemoryEventStore _testStore;

        public When_saving_same_aggregate_in_parallel_async()
        {
            var memoryCache = new MemoryCache();

            _testStore = new TestInMemoryEventStore();
            _rep1 = new CacheRepository(new Repository(_testStore), _testStore, memoryCache);
            _rep2 = new CacheRepository(new Repository(_testStore), _testStore, memoryCache);

            _aggregate = new TestAggregate(Guid.NewGuid());
            _rep1.SaveAsync(_aggregate).Wait();

            var t1 = new Task(() =>
            {
                for (var i = 0; i < 100; i++)
                {
                    var aggregate = _rep1.GetAsync<TestAggregate>(_aggregate.Id).Result;
                    aggregate.DoSomething();
                    _rep1.SaveAsync(aggregate).Wait();
                }
            });

            var t2 = new Task(() =>
            {
                for (var i = 0; i < 100; i++)
                {
                    var aggregate = _rep2.GetAsync<TestAggregate>(_aggregate.Id).Result;
                    aggregate.DoSomething();
                    _rep2.SaveAsync(aggregate).Wait();
                }
            });
            var t3 = new Task(() =>
            {
                for (var i = 0; i < 100; i++)
                {
                    var aggregate = _rep2.GetAsync<TestAggregate>(_aggregate.Id).Result;
                    aggregate.DoSomething();
                    _rep2.SaveAsync(aggregate).Wait();
                }
            });
            t1.Start();
            t2.Start();
            t3.Start();

            Task.WaitAll(t1, t2, t3);
        }

        [Fact]
        public void Should_not_get_more_than_one_event_with_same_id()
        {
            Assert.Equal(_testStore.Events.Count, _testStore.Events.Select(x => x.Version).Distinct().Count());
        }

        [Fact]
        public void Should_save_all_events()
        {
            Assert.Equal(301, _testStore.Events.Count);
        }
    }
}

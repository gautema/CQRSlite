using System;
using System.Linq;
using System.Threading.Tasks;
using CQRSlite.Caching;
using CQRSlite.Domain;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Caching
{
    public class When_saving_same_aggregate_in_parallel
    {
        private CacheRepository _rep1;
        private CacheRepository _rep2;
        private TestAggregate _aggregate;
        private TestInMemoryEventStore _testStore;

        public When_saving_same_aggregate_in_parallel()
        {
            var cache = new MemoryCache();

            _testStore = new TestInMemoryEventStore();
            _rep1 = new CacheRepository(new Repository(_testStore), _testStore, cache);
            _rep2 = new CacheRepository(new Repository(_testStore), _testStore, cache);

            _aggregate = new TestAggregate(Guid.NewGuid());
            _rep1.Save(_aggregate).Wait();

            var t1 = Task.Run(async () =>
            {
                for (var i = 0; i < 100; i++)
                {
                    var aggregate = await _rep1.Get<TestAggregate>(_aggregate.Id);
                    aggregate.DoSomething();
                    await _rep1.Save(aggregate);
                }
            });

            var t2 = Task.Run(async () =>
            {
                for (var i = 0; i < 100; i++)
                {
                    var aggregate = await _rep2.Get<TestAggregate>(_aggregate.Id);
                    aggregate.DoSomething();
                    await _rep2.Save(aggregate);
                }
            });
            var t3 = Task.Run(async () =>
            {
                for (var i = 0; i < 100; i++)
                {
                    var aggregate = await _rep2.Get<TestAggregate>(_aggregate.Id);
                    aggregate.DoSomething();
                    await _rep2.Save(aggregate);
                }
            });

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
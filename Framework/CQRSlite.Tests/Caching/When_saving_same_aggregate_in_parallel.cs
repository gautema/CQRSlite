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
        private readonly TestInMemoryEventStore _testStore;

        public When_saving_same_aggregate_in_parallel()
        {
            var cache = new MemoryCache();

            _testStore = new TestInMemoryEventStore();
            var rep1 = new CacheRepository(new Repository(_testStore), _testStore, cache);
            var rep2 = new CacheRepository(new Repository(_testStore), _testStore, cache);

            var aggregate1 = new TestAggregate(Guid.NewGuid());
            rep1.Save(aggregate1).Wait();

            var t1 = Task.Run(async () =>
            {
                for (var i = 0; i < 100; i++)
                {
                    var aggregate = await rep1.Get<TestAggregate>(aggregate1.Id);
                    aggregate.DoSomething();
                    await rep1.Save(aggregate);
                }
            });

            var t2 = Task.Run(async () =>
            {
                for (var i = 0; i < 100; i++)
                {
                    var aggregate = await rep2.Get<TestAggregate>(aggregate1.Id);
                    aggregate.DoSomething();
                    await rep2.Save(aggregate);
                }
            });
            var t3 = Task.Run(async () =>
            {
                for (var i = 0; i < 100; i++)
                {
                    var aggregate = await rep2.Get<TestAggregate>(aggregate1.Id);
                    aggregate.DoSomething();
                    await rep2.Save(aggregate);
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
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using CQRSlite.Cache;
using CQRSlite.Contracts.Domain;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.CachingTests
{
    public class When_saving_same_aggregate_in_parallel
    {
        private CacheRepository _rep1;
        private CacheRepository _rep2;
        private TestAggregate _aggregate;
        private TestInMemoryEventStore _testStore;

        [SetUp]
        public void Setup()
        {
            // This will clear the cache between runs.
            MemoryCache.Default.Remove(Guid.Empty.ToString());

            _testStore = new TestInMemoryEventStore();
            _rep1 = new CacheRepository(new Repository(_testStore,new TestEventPublisher()), _testStore);
            _rep2 = new CacheRepository(new Repository(_testStore,new TestEventPublisher()), _testStore);

            _aggregate = new TestAggregate(Guid.NewGuid());
            _rep1.Save(_aggregate);

            var t1 = new Task(() =>
                                  {
                                      for (var i = 0; i < 100; i++)
                                      {
                                          var aggregate = _rep1.Get<TestAggregate>(_aggregate.Id);
                                          aggregate.DoSomething();
                                          _rep1.Save(aggregate);
                                      }
                                  });

            var t2 = new Task(() =>
                                  {
                                      for (var i = 0; i < 100; i++)
                                      {
                                          var aggregate = _rep2.Get<TestAggregate>(_aggregate.Id);
                                          aggregate.DoSomething();
                                          _rep2.Save(aggregate);
                                      }
                                  });
            var t3 = new Task(() =>
                                  {
                                      for (var i = 0; i < 100; i++)
                                      {
                                          var aggregate = _rep2.Get<TestAggregate>(_aggregate.Id);
                                          aggregate.DoSomething();
                                          _rep2.Save(aggregate);
                                      }
                                  });
            t1.Start();
            t2.Start();
            t3.Start();

            Task.WaitAll(new[] {t1,t2, t3});
        }

        [Test]
        public void Should_not_get_more_than_one_event_with_same_id()
        {
            Assert.That(_testStore.Events.Select(x => x.Version).Distinct().Count(), Is.EqualTo(_testStore.Events.Count));
        }

        [Test]
        public void Should_save_all_events()
        {
            Assert.That(_testStore.Events.Count(), Is.EqualTo(301));
        }
    }
}
﻿using System;
using System.Linq;
using System.Threading.Tasks;
using CQRSlite.Cache;
using CQRSlite.Domain;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Cache
{
    public class When_saving_two_aggregates_in_parallel
    {
        private CacheRepository _rep1;
        private TestAggregate _aggregate1;
        private TestInMemoryEventStore _testStore;
        private TestAggregate _aggregate2;

        public When_saving_two_aggregates_in_parallel()
        {
            _testStore = new TestInMemoryEventStore();
            _rep1 = new CacheRepository(new Repository(_testStore), _testStore, new MemoryCache());

            _aggregate1 = new TestAggregate(GuidIdentity.Create());
            _aggregate2 = new TestAggregate(GuidIdentity.Create());

            _rep1.Save(_aggregate1).Wait();
            _rep1.Save(_aggregate2).Wait();

            var t1 = new Task(async () =>
            {
                for (var i = 0; i < 100; i++)
                {
                    var aggregate = await _rep1.Get<TestAggregate>(_aggregate1.Id);
                    aggregate.DoSomething();
                    await _rep1.Save(aggregate);
                }
            });

            var t2 = new Task(async () =>
            {
                for (var i = 0; i < 100; i++)
                {
                    var aggregate = await _rep1.Get<TestAggregate>(_aggregate2.Id);
                    aggregate.DoSomething();
                    await _rep1.Save(aggregate);
                }
            });
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);
        }
       
        [Fact]
        public void Should_not_get_more_than_one_event_with_same_id()
        {
            Assert.Equal(_testStore.Events.Count, _testStore.Events.Select(x => x.Version).Count());
        }

        [Fact]
        public void Should_save_all_events()
        {
            Assert.Equal(202, _testStore.Events.Count);
        }

        [Fact]
        public async Task Should_distibute_events_correct()
        {
            var aggregate1 = await _rep1.Get<TestAggregate>(_aggregate2.Id);
            Assert.Equal(100, aggregate1.DidSomethingCount);

            var aggregate2 = await _rep1.Get<TestAggregate>(_aggregate2.Id);
            Assert.Equal(100, aggregate2.DidSomethingCount);
        }
    }
}
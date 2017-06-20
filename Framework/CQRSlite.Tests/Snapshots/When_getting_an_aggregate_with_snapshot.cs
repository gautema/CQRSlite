﻿using System;
using CQRSlite.Domain;
using CQRSlite.Snapshots;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Snapshots
{
    public class When_getting_an_aggregate_with_snapshot
    {
        private TestSnapshotAggregate _aggregate;

        public When_getting_an_aggregate_with_snapshot()
        {
            var eventStore = new TestInMemoryEventStore();
            var snapshotStore = new TestSnapshotStore();
            var snapshotStrategy = new DefaultSnapshotStrategy();
            var snapshotRepository = new SnapshotRepository(snapshotStore, snapshotStrategy, new Repository(eventStore), eventStore);
            var session = new Session(snapshotRepository);

            _aggregate = session.Get<TestSnapshotAggregate>(GuidIdentity.Create()).Result;
        }

        [Fact]
        public void Should_restore()
        {
            Assert.True(_aggregate.Restored);
        }
    }
}

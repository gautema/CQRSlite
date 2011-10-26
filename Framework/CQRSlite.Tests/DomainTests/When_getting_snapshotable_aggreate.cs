using System;
using CQRSlite.Domain;
using CQRSlite.Infrastructure;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.DomainTests
{
	[TestFixture]
    public class When_getting_snapshotable_aggreate
    {
        private TestSnapshotStore _snapshotStore;
        private TestSnapshotAggregate _aggregate;

		[SetUp]
        public void Setup()
        {
            var eventStore = new TestEventStore();
            var eventPublisher = new TestEventPublisher();
            _snapshotStore = new TestSnapshotStore();
            var snapshotStrategy = new DefaultSnapshotStrategy();
            var session = new Session(eventStore, _snapshotStore, eventPublisher, snapshotStrategy);
            var rep = new Repository<TestSnapshotAggregate>(session, eventStore, _snapshotStore, snapshotStrategy);
            _aggregate = rep.Get(Guid.NewGuid());
        }

        [Test]
        public void Should_ask_for_snapshot()
        {
            Assert.True(_snapshotStore.VerifyGet);
        }

        [Test]
        public void Should_run_restore_method()
        {
            Assert.True(_aggregate.Restored);
        }
    }
}

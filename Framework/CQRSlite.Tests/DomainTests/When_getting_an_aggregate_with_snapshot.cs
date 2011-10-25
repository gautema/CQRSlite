using System;
using CQRSlite.Domain;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.DomainTests
{
	[TestFixture]
    public class When_getting_an_aggregate_with_snapshot
    {
        private TestSnapshotAggregate _aggregate;

		[SetUp]
        public void Setup()
        {
            var eventStore = new TestEventStore();
            var eventPublisher = new TestEventPublisher();
            var snapshotStore = new TestSnapshotStore();
            var session = new Session(eventStore, snapshotStore, eventPublisher);
            var rep = new Repository<TestSnapshotAggregate>(session, eventStore, snapshotStore);
            _aggregate = rep.Get(Guid.NewGuid());
        }

        [Test]
        public void Should_restore()
        {
            Assert.True(_aggregate.Restored);
        }
    }
}

using System;
using CQRSlite.Domain;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.DomainTests
{
	[TestFixture]
    public class When_getting_an_aggregate_with_snapshot
    {
        private TestSnapshotAggreagate _aggregate;

		[SetUp]
        public void Setup()
        {
            var eventStore = new TestEventStore();
            var eventPublisher = new TestEventPublisher();
            var snapshotStore = new TestSnapshotStore();
            var rep = new Repository<TestSnapshotAggreagate>(eventStore, snapshotStore, eventPublisher);
            _aggregate = rep.Get(Guid.NewGuid());
        }

        [Test]
        public void Should_restore()
        {
            Assert.True(_aggregate.Restored);
        }
    }
}

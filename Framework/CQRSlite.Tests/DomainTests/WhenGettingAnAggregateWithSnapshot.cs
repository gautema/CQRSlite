using System;
using CQRSlite.Domain;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.DomainTests
{
	[TestFixture]
    public class WhenGettingAnAggregateWithSnapshot
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
        public void ShouldRestore()
        {
            Assert.True(_aggregate.Restored);
        }
    }
}

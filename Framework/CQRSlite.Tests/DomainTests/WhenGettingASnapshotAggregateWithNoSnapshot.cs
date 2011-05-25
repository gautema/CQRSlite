using System;
using CQRSlite.Domain;
using CQRSlite.Eventing;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.DomainTests
{
	[TestFixture]
    public class WhenGettingASnapshotAggregateWithNoSnapshot
    {
        private TestSnapshotAggreagate _aggregate;

		[SetUp]
        public void Setup()
        {
            var eventStore = new TestEventStore();
            var eventPublisher = new TestEventPublisher();
            var snapshotStore = new NullSnapshotStore();
            var rep = new Repository<TestSnapshotAggreagate>(eventStore, snapshotStore, eventPublisher);
            _aggregate = rep.Get(Guid.NewGuid());
        }

        [Test]
        public void ShouldLoadEvents()
        {
            Assert.True(_aggregate.Loaded);
        }

        [Test]
        public void ShouldNotLoadSnapshot()
        {
            Assert.False(_aggregate.Restored);
        }
    }
}
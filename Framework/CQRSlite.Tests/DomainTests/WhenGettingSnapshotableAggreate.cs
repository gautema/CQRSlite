using System;
using CQRSlite.Domain;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.DomainTests
{
	[TestFixture]
    public class WhenGettingSnapshotableAggreate
    {
        private TestSnapshotStore _snapshotStore;
        private TestSnapshotAggreagate _aggregate;

		[SetUp]
        public void Setup()
        {
            var eventStore = new TestEventStore();
            var eventPublisher = new TestEventPublisher();
            _snapshotStore = new TestSnapshotStore();
            var rep = new Repository<TestSnapshotAggreagate>(eventStore, _snapshotStore,eventPublisher);
            _aggregate = rep.Get(Guid.NewGuid());
        }

        [Test]
        public void ShouldAskForSnapshot()
        {
            Assert.True(_snapshotStore.VerifyGet);
        }

        [Test]
        public void ShouldRunRestoreMethod()
        {
            Assert.True(_aggregate.Restored);
        }
    }
}

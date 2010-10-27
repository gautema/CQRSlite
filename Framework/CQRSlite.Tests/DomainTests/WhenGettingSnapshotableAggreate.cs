using System;
using CQRSlite.Domain;
using CQRSlite.Tests.TestSubstitutes;
using Xunit;

namespace CQRSlite.Tests.DomainTests
{
    public class WhenGettingSnapshotableAggreate
    {
        private TestSnapshotStore _snapshotStore;
        private TestSnapshotAggreagate _aggregate;

        public WhenGettingSnapshotableAggreate()
        {
            var eventStore = new TestEventStore();
            var eventPublisher = new TestEventPublisher();
            _snapshotStore = new TestSnapshotStore();
            var rep = new Repository<TestSnapshotAggreagate>(eventStore, _snapshotStore,eventPublisher);
            _aggregate = rep.Get(Guid.NewGuid());
        }

        [Fact]
        public void ShouldAskForSnapshot()
        {
            Assert.True(_snapshotStore.VerifyGet);
        }

        [Fact]
        public void ShouldRunRestoreMethod()
        {
            Assert.True(_aggregate.Restored);
        }
    }
}

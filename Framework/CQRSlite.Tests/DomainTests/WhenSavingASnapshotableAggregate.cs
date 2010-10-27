using CQRSlite.Domain;
using CQRSlite.Tests.TestSubstitutes;
using Xunit;

namespace CQRSlite.Tests.DomainTests
{
    public class WhenSavingASnapshotableAggregate
    {
        private TestSnapshotStore _snapshotStore;

        public WhenSavingASnapshotableAggregate()
        {
            var eventStore = new TestEventStore();
            var eventPublisher = new TestEventPublisher();
            _snapshotStore = new TestSnapshotStore();
            var rep = new Repository<TestSnapshotAggreagate>(eventStore, _snapshotStore, eventPublisher);
            var aggregate = new TestSnapshotAggreagate();
            for (int i = 0; i < 30; i++)
            {
                aggregate.DoSomething();
            }
            rep.Save(aggregate, 0);
        }

        [Fact]
        public void ShouldSaveSnapshot()
        {
            Assert.True(_snapshotStore.VerifySave);
        }

        [Fact]
        public void ShouldSaveLastVersionNumber()
        {
            Assert.Equal(30, _snapshotStore.SavedVersion);
        }
    }
}

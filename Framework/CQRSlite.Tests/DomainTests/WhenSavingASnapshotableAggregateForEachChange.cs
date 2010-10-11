using CQRSlite.Domain;
using CQRSlite.Tests.TestSubstitutes;
using Xunit;

namespace CQRSlite.Tests.DomainTests
{
    public class WhenSavingASnapshotableAggregateForEachChange
    {
        private TestSnapshotStore _snapshotStore;

        public WhenSavingASnapshotableAggregateForEachChange()
        {
            var eventStore = new TestEventStore();
            _snapshotStore = new TestSnapshotStore();
            var rep = new Repository<TestSnapshotAggreagate>(eventStore, _snapshotStore);
            var aggregate = new TestSnapshotAggreagate();
            for (int i = 0; i < 20; i++)
            {
                aggregate.DoSomething();
                rep.Save(aggregate, i);
            }
        }

        [Fact]
        public void ShouldSnapshot15thChange()
        {
            Assert.Equal(15, _snapshotStore.SavedVersion);
        }

        [Fact]
        public void ShouldNotSnapshotFirstEvent()
        {
            Assert.False(_snapshotStore.FirstSaved);
        }
    }
}
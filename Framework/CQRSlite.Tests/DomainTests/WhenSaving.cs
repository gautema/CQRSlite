using System.Linq;
using CQRSlite.Domain;
using CQRSlite.Eventing;
using CQRSlite.Tests.TestSubstitutes;
using Xunit;

namespace CQRSlite.Tests.DomainTests
{
    public class WhenSaving
    {
        private readonly TestEventStore _eventStore;
        private readonly TestAggregateNoParameterLessConstructor _aggregate;

        public WhenSaving()
        {
            _eventStore = new TestEventStore();
            var snapshotstore = new NullSnapshotStore();
            var rep = new Repository<TestAggregateNoParameterLessConstructor>(_eventStore, snapshotstore);
            _aggregate = new TestAggregateNoParameterLessConstructor(2);
            _aggregate.DoSomething();
            rep.Save(_aggregate, 1);
        }

        [Fact]
        public void ShouldSaveUncommitedChanges()
        {
            Assert.Equal(1, _eventStore.SavedEvents);
        }

        [Fact]
        public void ShouldMarkCommitedAfterSave()
        {
            Assert.Equal(0, _aggregate.GetUncommittedChanges().Count());
        }
    }
}

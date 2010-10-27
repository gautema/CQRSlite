using System;
using CQRSlite.Domain;
using CQRSlite.Eventing;
using CQRSlite.Tests.TestSubstitutes;
using Xunit;

namespace CQRSlite.Tests.DomainTests
{
    public class WhenSavingASnapshotableAggregateForEachChange
    {
        private readonly TestInMemorySnapshotStore _snapshotStore;
        private readonly Repository<TestSnapshotAggreagate> _rep;

        public WhenSavingASnapshotableAggregateForEachChange()
        {
            IEventStore eventStore = new TestInMemoryEventStore();
            var eventpubliser = new TestEventPublisher();
            _snapshotStore = new TestInMemorySnapshotStore();
            _rep = new Repository<TestSnapshotAggreagate>(eventStore, _snapshotStore, eventpubliser);
            var aggregate = new TestSnapshotAggreagate();
            for (int i = 0; i < 20; i++)
            {
                aggregate.DoSomething();
                _rep.Save(aggregate, i);
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

        [Fact]
        public void ShouldGetAggregateBackCorrect()
        {
            Assert.Equal(20, _rep.Get(Guid.Empty).Number);
        }
    }
}
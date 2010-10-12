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
            IEventRepository eventRepository = new TestInMemoryEventRepository();
            var eventpubliser = new TestEventPublisher();
            var eventStore = new EventStore(eventRepository,eventpubliser);
            _snapshotStore = new TestInMemorySnapshotStore();
            _rep = new Repository<TestSnapshotAggreagate>(eventStore, _snapshotStore);
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
            Assert.Equal(20, _rep.GetById(Guid.Empty).Number);
        }
    }
}
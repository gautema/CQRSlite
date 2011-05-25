using System;
using CQRSlite.Domain;
using CQRSlite.Eventing;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.DomainTests
{
	[TestFixture]
    public class WhenSavingASnapshotableAggregateForEachChange
    {
        private TestInMemorySnapshotStore _snapshotStore;
        private Repository<TestSnapshotAggreagate> _rep;

		[SetUp]
        public void Setup()        
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

        [Test]
        public void ShouldSnapshot15thChange()
        {
            Assert.AreEqual(15, _snapshotStore.SavedVersion);
        }

        [Test]
        public void ShouldNotSnapshotFirstEvent()
        {
            Assert.False(_snapshotStore.FirstSaved);
        }

        [Test]
        public void ShouldGetAggregateBackCorrect()
        {
            Assert.AreEqual(20, _rep.Get(Guid.Empty).Number);
        }
    }
}
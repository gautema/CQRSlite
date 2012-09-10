using System;
using CQRSlite.Contracts.Domain;
using CQRSlite.Contracts.Events;
using CQRSlite.Snapshots;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.SnapshottingTests
{
	[TestFixture]
    public class When_saving_a_snapshotable_aggregate_for_each_change
    {
        private TestInMemorySnapshotStore _snapshotStore;
	    private ISession _session;

	    [SetUp]
        public void Setup()        
		{
            IEventStore eventStore = new TestInMemoryEventStore();
            var eventPublisher = new TestEventPublisher();
            _snapshotStore = new TestInMemorySnapshotStore();
            var snapshotStrategy = new DefaultSnapshotStrategy();
            var repository = new SnapshotRepository(_snapshotStore, snapshotStrategy, new Repository(eventStore, eventPublisher), eventStore);
	        _session = new Session(repository);
            var aggregate = new TestSnapshotAggregate();

            for (int i = 0; i < 20; i++)
            {
                _session.Add(aggregate);
                aggregate.DoSomething();
                _session.Commit();
            }

        }

        [Test]
        public void Should_snapshot_15th_change()
        {
            Assert.AreEqual(15, _snapshotStore.SavedVersion);
        }

        [Test]
        public void Should_not_snapshot_first_event()
        {
            Assert.False(_snapshotStore.FirstSaved);
        }

        [Test]
        public void Should_get_aggregate_back_correct()
        {
            Assert.AreEqual(20, _session.Get<TestSnapshotAggregate>(Guid.Empty).Number);
        }
    }
}
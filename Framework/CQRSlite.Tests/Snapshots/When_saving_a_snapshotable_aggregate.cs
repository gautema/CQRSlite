using CQRSlite.Domain;
using CQRSlite.Snapshots;
using CQRSlite.Tests.Substitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.Snapshots
{
	[TestFixture]
    public class When_saving_a_snapshotable_aggregate
    {
        private TestSnapshotStore _snapshotStore;

		[SetUp]
        public void Setup()
        {
            var eventStore = new TestInMemoryEventStore();
            _snapshotStore = new TestSnapshotStore();
            var snapshotStrategy = new DefaultSnapshotStrategy();
            var repository = new SnapshotRepository(_snapshotStore, snapshotStrategy, new Repository(eventStore), eventStore);
            var session = new Session(repository);
            var aggregate = new TestSnapshotAggregate();
            for (var i = 0; i < 200; i++)
            {
                aggregate.DoSomething();
            }
            session.Add(aggregate);
		    session.Commit();
        }

        [Test]
        public void Should_save_snapshot()
        {
            Assert.True(_snapshotStore.VerifySave);
        }

        [Test]
        public void Should_save_last_version_number()
        {
            Assert.AreEqual(200, _snapshotStore.SavedVersion);
        }
    }
}

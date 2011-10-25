using CQRSlite.Domain;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.DomainTests
{
	[TestFixture]
    public class When_saving_a_snapshotable_aggregate
    {
        private TestSnapshotStore _snapshotStore;

		[SetUp]
        public void Setup()
        {
            var eventStore = new TestEventStore();
            var eventPublisher = new TestEventPublisher();
            _snapshotStore = new TestSnapshotStore();
		    var session = new Session(eventStore, _snapshotStore, eventPublisher);
		    var rep = new Repository<TestSnapshotAggregate>(session, eventStore, _snapshotStore);
            var aggregate = new TestSnapshotAggregate();
            for (int i = 0; i < 30; i++)
            {
                aggregate.DoSomething();
            }
            rep.Add(aggregate);
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
            Assert.AreEqual(30, _snapshotStore.SavedVersion);
        }
    }
}

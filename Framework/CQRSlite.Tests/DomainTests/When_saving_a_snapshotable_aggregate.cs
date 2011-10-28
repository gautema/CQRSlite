using CQRSlite.Domain;
using CQRSlite.Infrastructure;
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
            var snapshotStrategy = new DefaultSnapshotStrategy();
            var rep = new Repository<TestSnapshotAggregate>(eventStore, _snapshotStore, eventPublisher, snapshotStrategy);
            var aggregate = new TestSnapshotAggregate();
            for (int i = 0; i < 30; i++)
            {
                aggregate.DoSomething();
            }
            rep.Save(aggregate, 0);
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

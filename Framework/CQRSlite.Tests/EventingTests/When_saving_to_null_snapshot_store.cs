using CQRSlite.Eventing;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.EventingTests
{
	[TestFixture]
    public class When_saving_to_null_snapshot_store
    {
        private NullSnapshotStore _snapshotstore;
        private TestSnapshotAggregateSnapshot _snapshot;

		[SetUp]
        public void Setup()
        {
            _snapshotstore = new NullSnapshotStore();
            _snapshot = new TestSnapshotAggregateSnapshot();
            _snapshotstore.Save(_snapshot);
        }

        [Test]
        public void Should_not_return_snapshot()
        {
            var result = _snapshotstore.Get(_snapshot.Id);
            Assert.Null(result);
        }
    } 
}

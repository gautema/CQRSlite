using CQRSlite.Eventing;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.EventingTests
{
	[TestFixture]
    public class WhenSavingToNullSnapshotStore
    {
        private NullSnapshotStore _snapshotstore;
        private TestSnapshotAggreagateSnapshot _snapshot;

		[SetUp]
        public void Setup()
        {
            _snapshotstore = new NullSnapshotStore();
            _snapshot = new TestSnapshotAggreagateSnapshot();
            _snapshotstore.Save(_snapshot);
        }

        [Test]
        public void ShouldNotReturnSnapshot()
        {
            var result = _snapshotstore.Get(_snapshot.Id);
            Assert.Null(result);
        }
    } 
}

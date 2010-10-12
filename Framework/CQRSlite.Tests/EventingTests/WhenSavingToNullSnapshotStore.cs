using CQRSlite.Eventing;
using CQRSlite.Tests.TestSubstitutes;
using Xunit;

namespace CQRSlite.Tests.EventingTests
{
    public class WhenSavingToNullSnapshotStore
    {
        private NullSnapshotStore _snapshotstore;
        private TestSnapshotAggreagateSnapshot _snapshot;

        public WhenSavingToNullSnapshotStore()
        {
            _snapshotstore = new NullSnapshotStore();
            _snapshot = new TestSnapshotAggreagateSnapshot();
            _snapshotstore.Save(_snapshot);
        }

        [Fact]
        public void ShouldNotReturnSnapshot()
        {
            var result = _snapshotstore.Get(_snapshot.Id);
            Assert.Null(result);
        }
    } 
}

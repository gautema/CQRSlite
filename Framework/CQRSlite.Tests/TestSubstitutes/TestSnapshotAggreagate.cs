using CQRSlite.Domain;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestSnapshotAggreagate : SnapshotAggregateRoot<TestSnapshotAggreagateSnapshot>
    {
        public bool Restored;

        protected override TestSnapshotAggreagateSnapshot CreateSnapshot()
        {
            return new TestSnapshotAggreagateSnapshot();
        }

        protected override void RestoreFromSnapshot(TestSnapshotAggreagateSnapshot snapshot)
        {
            Restored = true;
        }
    }

    public class TestSnapshotAggreagateSnapshot : Snapshot
    {
        public TestSnapshotAggreagateSnapshot()
        {
            Version = 2;
        }
    }
}

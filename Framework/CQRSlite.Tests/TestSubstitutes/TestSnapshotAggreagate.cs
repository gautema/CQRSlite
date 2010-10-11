using CQRSlite.Domain;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestSnapshotAggreagate : SnapshotAggregateRoot<TestSnapshotAggreagateSnapshot>
    {
        public bool Restored { get; set; }
        public bool Loaded { get; set; }

        protected override TestSnapshotAggreagateSnapshot CreateSnapshot()
        {
            return new TestSnapshotAggreagateSnapshot {Version = Version};
        }

        protected override void RestoreFromSnapshot(TestSnapshotAggreagateSnapshot snapshot)
        {
            Restored = true;
        }

        private void Apply(TestAggregateDidSomething e)
        {
            Loaded = true;
            Version++;
        }

        public void DoSomething()
        {
            ApplyChange(new TestAggregateDidSomething());
        }

        protected int Version { get; set; }
    }

    public class TestSnapshotAggreagateSnapshot : Snapshot
    {
    }
}

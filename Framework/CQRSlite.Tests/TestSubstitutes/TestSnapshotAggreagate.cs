using CQRSlite.Domain;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestSnapshotAggreagate : SnapshotAggregateRoot<TestSnapshotAggreagateSnapshot>
    {
        public bool Restored { get; set; }
        public bool Loaded { get; set; }
        public int Number { get; set; }

        protected override TestSnapshotAggreagateSnapshot CreateSnapshot()
        {
            return new TestSnapshotAggreagateSnapshot{Number = Number};
        }

        protected override void RestoreFromSnapshot(TestSnapshotAggreagateSnapshot snapshot)
        {
            Restored = true;
        }

        private void Apply(TestAggregateDidSomething e)
        {
            Loaded = true;
            Number++;
        }

        public void DoSomething()
        {
            ApplyChange(new TestAggregateDidSomething());
        }

    }

    public class TestSnapshotAggreagateSnapshot : Snapshot
    {
        public int Number { get; set; }
    }
}

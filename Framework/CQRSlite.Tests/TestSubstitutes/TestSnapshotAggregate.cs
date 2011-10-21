using System;
using CQRSlite.Domain;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestSnapshotAggregate : SnapshotAggregateRoot<TestSnapshotAggregateSnapshot>
    {
        public bool Restored { get; set; }
        public bool Loaded { get; set; }
        public int Number { get; set; }

        protected override TestSnapshotAggregateSnapshot CreateSnapshot()
        {
            return new TestSnapshotAggregateSnapshot {Number = Number};
        }

        protected override void RestoreFromSnapshot(TestSnapshotAggregateSnapshot snapshot)
        {
            Number = snapshot.Number;
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

    public class TestSnapshotAggregateSnapshot : Snapshot
    {
        public int Number { get; set; }
    }
}

using System;
using CQRSlite.Snapshots;

namespace CQRSlite.Tests.Substitutes
{
    public class TestSnapshotAggregate : SnapshotAggregateRoot<TestSnapshotAggregateSnapshot>
    {
        public TestSnapshotAggregate() : base(Guid.NewGuid())
        {
            
        }
        public bool Restored { get; private set; }
        public bool Loaded { get; private set; }
        public int Number { get; private set; }

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

        public void SetId(Guid id)
        {
            Id = id;
        }
    }

    public class TestSnapshotAggregateSnapshot : Snapshot
    {
        public int Number { get; set; }
    }
}

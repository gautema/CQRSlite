using System;
using CQRSlite.Domain;

namespace CQRSlite.Snapshots
{
    public abstract class SnapshotAggregateRoot<T>: AggregateRoot where T : Snapshot
    {
        protected SnapshotAggregateRoot(Guid id) : base(id)
        {
        }

        public T GetSnapshot()
        {
            var snapshot = CreateSnapshot();
            snapshot.Id = Id;
            return snapshot;
        }

        public void Restore(T snapshot)
        {
            Id = snapshot.Id;
            Version = snapshot.Version;
            RestoreFromSnapshot(snapshot);
        }

        protected abstract T CreateSnapshot();
        protected abstract void RestoreFromSnapshot(T snapshot);
    }

}

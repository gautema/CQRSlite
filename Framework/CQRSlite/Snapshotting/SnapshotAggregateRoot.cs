using CQRSlite.Domain;

namespace CQRSlite.Snapshotting
{
    /// <summary>
    /// Class to inherit aggregates that should be snapshotted from.
    /// </summary>
    /// <typeparam name="T">Type of memento object capturing aggregate state</typeparam>
    public abstract class SnapshotAggregateRoot<T> : AggregateRoot where T : Snapshot
    {
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

using System;

namespace CQRSlite.Eventing
{
    public abstract class Snapshotable<T> : ISnapshotable  where T : Snapshot
    {
        public abstract Guid Id { get; protected set; }
        public int Version { get; internal set; }

        public T GetSnapshot()
        {
            var snapshot = CreateSnapshot();
            snapshot.Id = Id;
            snapshot.Version = Version;
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

    public interface ISnapshotable {}
}

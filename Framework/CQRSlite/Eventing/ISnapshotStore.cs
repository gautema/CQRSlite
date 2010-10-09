using System;

namespace CQRSlite.Eventing
{
    public interface ISnapshotStore
    {
        T Get<T>(Guid id) where T : Snapshot;
        void Save<T>(T aggregateRoot) where T : Snapshot;
    }
}

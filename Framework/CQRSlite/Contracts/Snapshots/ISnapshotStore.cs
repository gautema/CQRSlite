using System;
using CQRSlite.Snapshots;

namespace CQRSlite.Contracts.Snapshots
{
    public interface ISnapshotStore
    {
        Snapshot Get(Guid id);
        void Save(Snapshot snapshot);
    }
}

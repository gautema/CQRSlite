using System;
using CQRSlite.Snapshotting;

namespace CQRSlite.Eventing
{
    public interface ISnapshotStore
    {
        Snapshot Get(Guid id);
        void Save(Snapshot snapshot);
    }
}

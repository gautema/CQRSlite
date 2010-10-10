using System;
using CQRSlite.Domain;

namespace CQRSlite.Eventing
{
    public interface ISnapshotStore
    {
        Snapshot Get(Guid id);
        void Save(Snapshot snapshot);
    }
}

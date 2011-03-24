using System;
using CQRSlite.Domain;

namespace CQRSlite.Eventing
{
    public class NullSnapshotStore : ISnapshotStore
    {
        public Snapshot Get(Guid id)
        {
            return null;
        }

        public void Save(Snapshot snapshot)
        {
            
        }
    }
}
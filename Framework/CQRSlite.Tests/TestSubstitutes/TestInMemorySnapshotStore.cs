using System;
using CQRSlite.Contracts.Snapshots;
using CQRSlite.Snapshots;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestInMemorySnapshotStore : ISnapshotStore 
    {
        public Snapshot Get(Guid id)
        {
            return _snapshot;
        }

        public void Save(Snapshot snapshot)
        {
            if(snapshot.Version == 0)
                FirstSaved = true;
            SavedVersion = snapshot.Version;
            _snapshot = snapshot;
        }

        private Snapshot _snapshot;
        public int SavedVersion { get; set; }
        public bool FirstSaved { get; set; }
    }
}
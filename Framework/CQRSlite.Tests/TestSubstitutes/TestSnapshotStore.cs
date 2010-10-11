using System;
using CQRSlite.Domain;
using CQRSlite.Eventing;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestSnapshotStore : ISnapshotStore
    {
        public bool VerifyGet { get; set; }
        public bool VerifySave { get; set; }
        public int SavedVersion { get; set; }
        public bool FirstSaved { get; set; }

        public Snapshot Get(Guid id)
        {
            VerifyGet = true;
            return new TestSnapshotAggreagateSnapshot();
        }

        public void Save(Snapshot snapshot)
        {
            VerifySave = true;
            if (snapshot.Version == 0) FirstSaved = true;

            SavedVersion = snapshot.Version;
        }
    }
}

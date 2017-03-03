using System;
using System.Threading.Tasks;
using CQRSlite.Snapshots;

namespace CQRSlite.Tests.Substitutes
{
    public class TestSnapshotStore : ISnapshotStore
    {
        public bool VerifyGet { get; private set; }
        public bool VerifySave { get; private set; }
        public int SavedVersion { get; private set; }

        public Task<Snapshot> Get(Guid id)
        {
            VerifyGet = true;
            return Task.FromResult((Snapshot)new TestSnapshotAggregateSnapshot());
        }

        public Task Save(Snapshot snapshot)
        {
            VerifySave = true;
            SavedVersion = snapshot.Version;
            return Task.CompletedTask;
        }
    }
}

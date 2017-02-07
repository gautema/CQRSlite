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

        public Snapshot Get(Guid id)
        {
            VerifyGet = true;
            return new TestSnapshotAggregateSnapshot();
        }

        public void Save(Snapshot snapshot)
        {
            VerifySave = true;
            SavedVersion = snapshot.Version;
        }

        public Task<Snapshot> GetAsync(Guid id)
        {
            return Task.FromResult<Snapshot>(Get(id));
        }

        public Task SaveAsync(Snapshot snapshot)
        {
            Save(snapshot);
            return Task.FromResult(0);
        }
    }
}

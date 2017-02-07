using System;
using System.Threading.Tasks;
using CQRSlite.Snapshots;

namespace CQRSlite.Tests.Substitutes
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

        public Task<Snapshot> GetAsync(Guid id)
        {
            return Task.FromResult<Snapshot>(Get(id));
        }

        public Task SaveAsync(Snapshot snapshot)
        {
            Save(snapshot);
            return Task.FromResult(0);
        }

        private Snapshot _snapshot;
        public int SavedVersion { get; private set; }
        public bool FirstSaved { get; private set; }
    }
}
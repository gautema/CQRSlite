using System;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Snapshotting;

namespace CQRSlite.Tests.Substitutes
{
    public class TestInMemorySnapshotStore : ISnapshotStore 
    {
        public Task<Snapshot> Get(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_snapshot);
        }

        public Task Save(Snapshot snapshot, CancellationToken cancellationToken = default)
        {
            if(snapshot.Version == 0)
                FirstSaved = true;
            SavedVersion = snapshot.Version;
            _snapshot = snapshot;
            return Task.CompletedTask;
        }

        private Snapshot _snapshot;
        public int SavedVersion { get; private set; }
        public bool FirstSaved { get; private set; }
    }
}
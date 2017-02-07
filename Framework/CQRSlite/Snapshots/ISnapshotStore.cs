using System;
using System.Threading.Tasks;

namespace CQRSlite.Snapshots
{
    public interface ISnapshotStore
    {
        Snapshot Get(Guid id);
        Task<Snapshot> GetAsync(Guid id);
        void Save(Snapshot snapshot);
        Task SaveAsync(Snapshot snapshot);
    }
}

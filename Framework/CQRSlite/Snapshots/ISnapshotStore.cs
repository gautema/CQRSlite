using System;
using System.Threading.Tasks;

namespace CQRSlite.Snapshots
{
    public interface ISnapshotStore
    {
        Task<Snapshot> Get(Guid id);
        Task Save(Snapshot snapshot);
    }
}

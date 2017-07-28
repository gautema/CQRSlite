using System;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSlite.Snapshotting
{
    public interface ISnapshotStore
    {
        Task<Snapshot> Get(Guid id, CancellationToken cancellationToken = default(CancellationToken));
        Task Save(Snapshot snapshot, CancellationToken cancellationToken = default(CancellationToken));
    }
}

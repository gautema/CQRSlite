using System;
using System.Threading.Tasks;
using CQRSlite.Domain;

namespace CQRSlite.Snapshots
{
    public interface ISnapshotStore
    {
        Task<Snapshot> Get(IIdentity id);
        Task Save(Snapshot snapshot);
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSlite.Snapshotting
{
    /// <summary>
    /// Defines the methods needed from the snapshot store.
    /// </summary>
    public interface ISnapshotStore
    {
        /// <summary>
        /// Get snapshot from store
        /// </summary>
        /// <param name="id">Id of aggregate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task with snapshot</returns>
        Task<Snapshot> Get(Guid id, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Save snapshot to store
        /// </summary>
        /// <param name="snapshot">The snapshot to save</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task of operation</returns>
        Task Save(Snapshot snapshot, CancellationToken cancellationToken = default(CancellationToken));
    }
}

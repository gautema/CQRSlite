using System;
using CQRSlite.Domain;

namespace CQRSlite.Snapshotting
{
    /// <summary>
    /// Defines strategy for when to take snapshot.
    /// </summary>
    public interface ISnapshotStrategy
    {
        /// <summary>
        /// Check if aggregate should be taken snapshot of now.
        /// </summary>
        /// <param name="aggregate">Aggregate to be taken snapshot of</param>
        /// <returns>If snapshot should be taken</returns>
        bool ShouldMakeSnapShot(AggregateRoot aggregate);

        /// <summary>
        /// Check if aggregate should be taken snapshot of
        /// </summary>
        /// <param name="aggregateType"></param>
        /// <returns>If snapshot can be taken of this aggregate</returns>
        bool IsSnapshotable(Type aggregateType);
    }
}
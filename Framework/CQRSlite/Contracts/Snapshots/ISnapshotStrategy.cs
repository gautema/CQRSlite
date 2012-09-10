using System;
using CQRSlite.Domain;

namespace CQRSlite.Contracts.Snapshots
{
    public interface ISnapshotStrategy
    {
        bool ShouldMakeSnapShot(AggregateRoot aggregate);
        bool IsSnapshotable(Type aggregateType);
    }
}
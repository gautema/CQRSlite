using System;
using CQRSlite.Domain;

namespace CQRSlite.Snapshotting
{
    public interface ISnapshotStrategy
    {
        bool ShouldMakeSnapShot(AggregateRoot aggregate);
        bool IsSnapshotable(Type aggregateType);
    }
}
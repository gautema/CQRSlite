using System;
using CQRSlite.Domain;

namespace CQRSlite.Infrastructure
{
    public interface ISnapshotStrategy
    {
        bool ShouldMakeSnapShot(AggregateRoot aggregate);
        bool IsSnapshotable(Type aggregateType);
    }
}
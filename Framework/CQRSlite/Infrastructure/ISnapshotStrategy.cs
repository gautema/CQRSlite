using System;
using CQRSlite.Domain;

namespace CQRSlite.Infrastructure
{
    public interface ISnapshotStrategy
    {
        bool ShouldMakeSnapShot(AggregateRoot aggregate, int expectedVersion);
        bool IsSnapshotable(Type aggregateType);
    }
}
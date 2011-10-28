using System;
using CQRSlite.Domain;

namespace CQRSlite.Infrastructure
{
    public class NullSnapshotStrategy : ISnapshotStrategy
    {
        public bool ShouldMakeSnapShot(AggregateRoot aggregate)
        {
            return false;
        }

        public bool IsSnapshotable(Type aggregateType)
        {
            return false;
        }
    }
}
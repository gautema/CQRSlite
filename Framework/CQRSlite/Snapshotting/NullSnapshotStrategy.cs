using System;
using CQRSlite.Domain;

namespace CQRSlite.Snapshotting
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
using System;
using System.Reflection;
using CQRSlite.Domain;

namespace CQRSlite.Snapshotting
{
    /// <inheritdoc />
    /// <summary>
    /// Default implementation of snapshot strategy interface/
    /// Snapshots aggregates of type SnapshotAggregateRoot every 100th event.
    /// </summary>
    public class DefaultSnapshotStrategy : ISnapshotStrategy
    {
        private readonly ushort _snapshotInterval = 100;

        public DefaultSnapshotStrategy() { }
        public DefaultSnapshotStrategy(ushort interval)
        {
            _snapshotInterval = interval;
        }

        public bool IsSnapshotable(Type aggregateType)
        {
            while (true)
            {
                if (aggregateType.GetTypeInfo().BaseType == null) return false;
                if (aggregateType.GetTypeInfo().BaseType.GetTypeInfo().IsGenericType && 
                    aggregateType.GetTypeInfo().BaseType.GetGenericTypeDefinition() == typeof(SnapshotAggregateRoot<>)) 
                    return true;
                aggregateType = aggregateType.GetTypeInfo().BaseType;
            }
        }

        public bool ShouldMakeSnapShot(AggregateRoot aggregate)
        {
            if (!IsSnapshotable(aggregate.GetType()))
                return false;

            var i = aggregate.Version;
            for (var j = 0; j < aggregate.GetUncommittedChanges().Length; j++)
                if (++i % _snapshotInterval == 0 && i != 0)
                    return true;
            return false;
        }
    }
}
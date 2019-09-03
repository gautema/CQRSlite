using System;
using System.Reflection;
using CQRSlite.Domain;

namespace CQRSlite.Snapshotting
{
    /// <inheritdoc />
    /// <summary>
    /// Snapshot strategy with configurable snapshot interval.
    /// </summary>
    public class ConfigurableIntervalSnapshotStrategy : ISnapshotStrategy
    {
        private readonly int snapshotInterval;

        /// <summary>
        /// Snapshot strategy with configurable snapshot interval.
        /// </summary>
        /// <param name="snapshotInterval">Snapshot interval. Must be greater than zero.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="snapshotInterval"/> is less then or equal to zero.</exception>
        public ConfigurableIntervalSnapshotStrategy(int snapshotInterval)
        {
            if (snapshotInterval <= 0)
                throw new ArgumentOutOfRangeException(nameof(snapshotInterval), "Must be positive");

            this.snapshotInterval = snapshotInterval;
        }

        public bool IsSnapshotable(Type aggregateType)
        {
            var baseType = aggregateType.GetTypeInfo().BaseType;

            if (baseType == null)
                return false;

            if (baseType.GetTypeInfo().IsGenericType &&
                baseType.GetGenericTypeDefinition() == typeof(SnapshotAggregateRoot<>))
                return true;

            return IsSnapshotable(baseType);
        }

        public bool ShouldMakeSnapShot(AggregateRoot aggregate)
        {
            if (aggregate == null)
                return false;

            if (!IsSnapshotable(aggregate.GetType()))
                return false;

            var uncommittedChangesCount = aggregate.GetUncommittedChanges().Length;

            if (uncommittedChangesCount == 0)
                return false;

            if (uncommittedChangesCount >= snapshotInterval)
                return true;

            var aggregateVersion = aggregate.Version;
            if ((aggregateVersion % snapshotInterval) + uncommittedChangesCount >= snapshotInterval)
                return true;

            return false;
        }
    }
}
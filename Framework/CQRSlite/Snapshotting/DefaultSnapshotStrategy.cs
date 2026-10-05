using CQRSlite.Domain;

namespace CQRSlite.Snapshotting;

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
        if (interval == 0)
            throw new ArgumentOutOfRangeException(nameof(interval), "Snapshot interval must be greater than 0");
        _snapshotInterval = interval;
    }

    public bool IsSnapshotable(Type aggregateType)
    {
        for (var baseType = aggregateType.BaseType; baseType != null; baseType = baseType.BaseType)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(SnapshotAggregateRoot<>))
                return true;
        }
        return false;
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
namespace CQRSlite.Snapshotting
{
    /// <inheritdoc cref="ISnapshotStrategy" />
    /// <summary>
    /// Default implementation of snapshot strategy interface/
    /// Snapshots aggregates of type SnapshotAggregateRoot every 100th event.
    /// </summary>
    public class DefaultSnapshotStrategy : ConfigurableIntervalSnapshotStrategy, ISnapshotStrategy
    {
        public DefaultSnapshotStrategy()
            : base(100)
        {
        }
    }
}
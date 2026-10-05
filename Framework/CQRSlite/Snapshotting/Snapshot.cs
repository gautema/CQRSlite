namespace CQRSlite.Snapshotting;

/// <summary>
/// A memento object of an aggregate in a version.
/// </summary>
public abstract class Snapshot
{
    public Guid Id { get; set; }
    public int Version { get; set; }
}

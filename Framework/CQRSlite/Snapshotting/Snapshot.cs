using System;

namespace CQRSlite.Snapshotting
{
    public abstract class Snapshot
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
    }
}

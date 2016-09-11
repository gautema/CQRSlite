using System;

namespace CQRSlite.Snapshots
{
    public abstract class Snapshot
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
    }
}

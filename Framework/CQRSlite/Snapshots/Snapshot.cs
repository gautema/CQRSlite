using System;
using CQRSlite.Domain;

namespace CQRSlite.Snapshots
{
    public abstract class Snapshot
    {
        public IIdentity Id { get; set; }
        public int Version { get; set; }
    }
}

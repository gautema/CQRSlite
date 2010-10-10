using System;
using CQRSlite.Domain;
using CQRSlite.Eventing;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestSnapshotStore : ISnapshotStore
    {
        public bool VerifyGet { get; set; }

        public Snapshot Get(Guid id)
        {
            VerifyGet = true;
            return new TestSnapshotAggreagateSnapshot();
        }

        public void Save(Snapshot snapshot)
        {
            throw new NotImplementedException();
        }
    }
}

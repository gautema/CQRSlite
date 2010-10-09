using System;
using CQRSlite.Eventing;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestSnapshotStore : ISnapshotStore
    {
        public bool VerifyGet { get; set; }
        public T Get<T>(Guid id) where T : Snapshot
        {
            VerifyGet = true;
            return null;
        }

        public void Save<T>(T aggregateRoot) where T : Snapshot
        {
            throw new NotImplementedException();
        }
    }
}

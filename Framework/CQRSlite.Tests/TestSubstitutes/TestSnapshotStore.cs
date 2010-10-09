using System;
using CQRSlite.Eventing;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestSnapshotStore : ISnapshotStore<TestAggregate>
    {
        public Snapshot<TestAggregate> Get(Guid id  )
        {
            VerifyGet = true;
            return null;
        }

        public void Save(TestAggregate aggregateRoot)
        {
            throw new NotImplementedException();
        }

        public bool VerifyGet { get; set; }
    }
}

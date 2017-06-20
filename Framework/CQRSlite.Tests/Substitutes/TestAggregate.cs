using System;
using CQRSlite.Domain;

namespace CQRSlite.Tests.Substitutes
{
    public class TestAggregate : AggregateRoot
    {
        private TestAggregate() { }
        public TestAggregate(IIdentity id)
        {
            Id = id;
            ApplyChange(new TestAggregateCreated { Id = Id });
        }

        public int DidSomethingCount;

        public void DoSomething()
        {
            ApplyChange(new TestAggregateDidSomething { Id = Id });
        }

        public void DoSomethingElse()
        {
            ApplyChange(new TestAggregateDidSomeethingElse { Id = Id });
        }

        public void Apply(TestAggregateDidSomething e)
        {
            DidSomethingCount++;
        }

    }
}

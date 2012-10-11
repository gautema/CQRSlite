using System;
using CQRSlite.Domain;

namespace CQRSlite.Tests.Substitutes
{
    public class TestAggregate : AggregateRoot
    {
        private TestAggregate() : base(Guid.Empty){}
        public TestAggregate(Guid id) : base(id)
        {
            ApplyChange(new TestAggregateCreated(id));
        }
        public int DidSomethingCount;

        public void DoSomething()
        {
            ApplyChange(new TestAggregateDidSomething());
        }

        public void DoSomethingElse()
        {
            ApplyChange(new TestAggregateDidSomeethingElse());
        }

        public void Apply(TestAggregateDidSomething e)
        {
            DidSomethingCount++;
        }

    }
}

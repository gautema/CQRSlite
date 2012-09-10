using System;
using CQRSlite.Contracts.Domain;
using CQRSlite.Contracts.Messages;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestAggregate : AggregateRoot
    {
        private TestAggregate(){}
        public TestAggregate(Guid id)
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

    public class TestAggregateCreated : Event
    {
        private readonly Guid _id;

        public TestAggregateCreated(Guid id)
        {
            _id = id;
        }
    }

    public class TestAggregateNoParameterLessConstructor : AggregateRoot
    {
        public TestAggregateNoParameterLessConstructor(int i, Guid? id = null)
        {
            if (id != null)
                Id = id.Value;
        }

        public void DoSomething()
        {
            ApplyChange(new TestAggregateDidSomething());
        }
    }
}

using System;
using CQRSlite.Domain;
using CQRSlite.Eventing;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestAggregate : AggregateRoot
    {
        private TestAggregate(){}
        public TestAggregate(Guid id)
        {
            ApplyChange(new TestAggregateCreated(id));
        }
        public int I;

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
            I++;
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

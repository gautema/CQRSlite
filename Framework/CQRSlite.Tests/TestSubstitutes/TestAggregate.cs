using System;
using CQRSlite.Domain;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestAggregate : AggregateRoot
    {
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

        public void SetVersion(int version)
        {
            Version = version;
        }
    }
}

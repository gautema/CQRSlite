using System;
using CQRSlite.Domain;

namespace CQRSlite.Tests.Substitutes
{
    public class TestAggregateNoParameterLessConstructor : AggregateRoot
    {
        public TestAggregateNoParameterLessConstructor(int i, Guid id) : base(id)
        {
        }

        public void DoSomething()
        {
            ApplyChange(new TestAggregateDidSomething());
        }
    }
}
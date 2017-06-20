using System;
using CQRSlite.Domain;

namespace CQRSlite.Tests.Substitutes
{
    public class TestAggregateNoParameterLessConstructor : AggregateRoot
    {
        public TestAggregateNoParameterLessConstructor(int i, IIdentity id = null)
        {
            Id = id ?? GuidIdentity.Create();
        }

        public void DoSomething()
        {
            ApplyChange(new TestAggregateDidSomething{ Id = Id });
        }
    }
}
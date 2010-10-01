using System;
using SimpleCQRS.Commanding;

namespace SimpleCQRS.Tests.TestSubstitutes
{
    public class TestAggregateDoSomething : Command
    {

    }

    public class TestAggregateDoSomethingHandler : IHandles<TestAggregateDoSomething> {
        public void Handle(TestAggregateDoSomething message)
        {
            throw new NotImplementedException();
        }

    }
    public class TestAggregateDoSomethingElseHandler : IHandles<TestAggregateDoSomething>
    {
        public void Handle(TestAggregateDoSomething message)
        {
            throw new NotImplementedException();
        }
    }
}
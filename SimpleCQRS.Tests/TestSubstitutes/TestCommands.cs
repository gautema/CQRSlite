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
            TimesRun++;
        }

        public int TimesRun { get; set; }
    }
    public class TestAggregateDoSomethingElseHandler : IHandles<TestAggregateDoSomething>
    {
        public void Handle(TestAggregateDoSomething message)
        {
            TimesRun++;
        }
        public int TimesRun { get; set; }
    }
}
using CQRSlite.Eventing;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestAggregateDidSomething : Event
    {
        
    }
    public class TestAggregateDidSomeethingElse : Event
    {

    }

    public class TestAggregateDidSomethingHandler : IHandles<TestAggregateDidSomething>
    {
        public void Handle(TestAggregateDidSomething message)
        {
            TimesRun++;
        }

        public int TimesRun { get; set; }
    }
}

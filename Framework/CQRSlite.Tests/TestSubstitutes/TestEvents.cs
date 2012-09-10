using CQRSlite.Contracts.Handlers;
using CQRSlite.Contracts.Messages;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestAggregateDidSomething : Event
    {
        
    }
    public class TestAggregateDidSomeethingElse : Event
    {

    }

    public class TestAggregateDidSomethingHandler : HandlesEvent<TestAggregateDidSomething>
    {
        public void Handle(TestAggregateDidSomething message)
        {
            lock (message)
            {
                TimesRun++;
            }
        }

        public int TimesRun { get; set; }
    }
}

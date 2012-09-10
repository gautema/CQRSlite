using CQRSlite.Contracts.Bus.Handlers;
using CQRSlite.Contracts.Bus.Messages;

namespace CQRSlite.Tests.Substitutes
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

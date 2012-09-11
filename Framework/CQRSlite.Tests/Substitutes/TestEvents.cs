using CQRSlite.Events;
using CQRSlite.Messages;

namespace CQRSlite.Tests.Substitutes
{
    public class TestAggregateDidSomething : Event
    {
        
    }
    public class TestAggregateDidSomeethingElse : Event
    {

    }

    public class TestAggregateDidSomethingHandler : IEventHandler<TestAggregateDidSomething>
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

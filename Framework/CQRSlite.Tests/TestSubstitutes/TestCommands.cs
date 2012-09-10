using CQRSlite.Commanding;
using CQRSlite.Handlers;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestAggregateDoSomething : Command
    {

    }

    public class TestAggregateDoSomethingHandler : HandlesCommand<TestAggregateDoSomething> 
    {
        public void Handle(TestAggregateDoSomething message)
        {
            TimesRun++;
        }

        public int TimesRun { get; set; }
    }
	public class TestAggregateDoSomethingElseHandler : HandlesCommand<TestAggregateDoSomething>
    {
        public void Handle(TestAggregateDoSomething message)
        {

        }
    }
}
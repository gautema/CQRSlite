using CQRSlite.Commands;
using CQRSlite.Messages;

namespace CQRSlite.Tests.Substitutes
{
    public class TestAggregateDoSomething : Command
    {

    }

    public class TestAggregateDoSomethingHandler : ICommandHandler<TestAggregateDoSomething> 
    {
        public void Handle(TestAggregateDoSomething message)
        {
            TimesRun++;
        }

        public int TimesRun { get; set; }
    }
	public class TestAggregateDoSomethingElseHandler : ICommandHandler<TestAggregateDoSomething>
    {
        public void Handle(TestAggregateDoSomething message)
        {

        }
    }
}
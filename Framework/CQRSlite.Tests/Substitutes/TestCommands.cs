using System;
using System.Threading.Tasks;
using CQRSlite.Commands;
using CQRSlite.Domain.Exception;

namespace CQRSlite.Tests.Substitutes
{
    public class TestAggregateDoSomething : ICommand
    {
        public Guid Id { get; set; }
        public int ExpectedVersion { get; set; }
    }

    public class TestAggregateDoSomethingHandler : ICommandHandler<TestAggregateDoSomething> 
    {
        public Task Handle(TestAggregateDoSomething message)
        {
            if(message.ExpectedVersion != TimesRun)
                throw new ConcurrencyException(message.Id);
            TimesRun++;
            return Task.CompletedTask;
        }

        public int TimesRun { get; set; }
    }
	public class TestAggregateDoSomethingElseHandler : ICommandHandler<TestAggregateDoSomething>
    {
        public Task Handle(TestAggregateDoSomething message)
        {
            return Task.CompletedTask;
        }
    }
}
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
        public bool LongRunning { get; set; }
    }

    public class TestAggregateDoSomethingHandler : ICommandHandler<TestAggregateDoSomething> 
    {
        public async Task Handle(TestAggregateDoSomething message)
        {
            if (message.LongRunning)
                await Task.Delay(50);
            if(message.ExpectedVersion != TimesRun)
                throw new ConcurrencyException(message.Id);
            TimesRun++;
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
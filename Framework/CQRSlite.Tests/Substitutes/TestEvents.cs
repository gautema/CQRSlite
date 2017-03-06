using System;
using System.Threading.Tasks;
using CQRSlite.Domain.Exception;
using CQRSlite.Events;

namespace CQRSlite.Tests.Substitutes
{
    public class TestAggregateDidSomething : IEvent
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public bool LongRunning { get; set; }
    }
    public class TestAggregateDidSomeethingElse : IEvent
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }

    public class TestAggregateDidSomethingHandler : IEventHandler<TestAggregateDidSomething>
    {
        public async Task Handle(TestAggregateDidSomething message)
        {
            if (message.LongRunning)
                await Task.Delay(50);
            lock (message)
            {
                if(message.Version == -10)
                    throw new ConcurrencyException(message.Id);
                TimesRun++;
            }
        }

        public int TimesRun { get; private set; }
    }
}

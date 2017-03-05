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
    }
    public class TestAggregateDidSomeethingElse : IEvent
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }

    public class TestAggregateDidSomethingHandler : IEventHandler<TestAggregateDidSomething>
    {
        public Task Handle(TestAggregateDidSomething message)
        {
            lock (message)
            {
                if(message.Version == -10)
                    throw new ConcurrencyException(message.Id);
                TimesRun++;
                return Task.CompletedTask;
            }
        }

        public int TimesRun { get; private set; }
    }
}

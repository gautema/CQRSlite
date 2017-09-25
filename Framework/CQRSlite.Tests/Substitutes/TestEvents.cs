using System;
using System.Threading;
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

    internal class TestAggregateDidSomethingInternal : IEvent
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public bool LongRunning { get; set; }
    }

    public class TestAggregateDidSomethingElse : IEvent
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }

    public class TestAggregateDidSomethingHandler : ICancellableEventHandler<TestAggregateDidSomething>,
        IEventHandler<TestAggregateDidSomethingElse>
    {
        public async Task Handle(TestAggregateDidSomething message, CancellationToken token)
        {
            if (message.LongRunning)
                await Task.Delay(50, token);
            lock (message)
            {
                if (message.Version == -10)
                    throw new ConcurrencyException(message.Id);
                TimesRun++;
                Token = token;
            }
        }

        public Task Handle(TestAggregateDidSomethingElse message)
        {
            TimesRun++;
            return Task.CompletedTask;
        }


        public CancellationToken Token { get; private set; }
        public int TimesRun { get; private set; }
    }

    internal class TestAggregateDidSomethingInternalHandler : ICancellableEventHandler<TestAggregateDidSomethingInternal>
    {
        public Task Handle(TestAggregateDidSomethingInternal message, CancellationToken token)
        {
            TimesRun++;
            Token = token;
            return Task.CompletedTask;
        }

        public CancellationToken Token { get; private set; }
        public int TimesRun { get; private set; }
    }

    public class AllHandler : IEventHandler<IEvent>
    {
        public Task Handle(IEvent message)
        {
            TimesRun++;
            return Task.CompletedTask;
        }

        public int TimesRun { get; private set; }
    }
}

using System;
using System.Threading.Tasks;
using CQRSlite.Events;

namespace CQRSlite.Tests.Substitutes
{
    public class TestEventPublisher: IEventPublisher {
        public void Publish<T>(T @event) where T : IEvent
        {
            Published++;
        }

        public Task PublishAsync<T>(T @event) where T : IEvent
        {
            Publish<T>(@event);
            return Task.FromResult(0);
        }

        public int Published { get; private set; }
    }
}
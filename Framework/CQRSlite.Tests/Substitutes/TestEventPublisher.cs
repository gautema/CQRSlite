using System.Threading.Tasks;
using CQRSlite.Events;

namespace CQRSlite.Tests.Substitutes
{
    public class TestEventPublisher: IEventPublisher {
        public Task Publish<T>(T @event) where T : class, IEvent
        {
            Published++;
            return Task.CompletedTask;
        }

        public int Published { get; private set; }
    }
}
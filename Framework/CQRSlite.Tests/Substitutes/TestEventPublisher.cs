using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Events;

namespace CQRSlite.Tests.Substitutes
{
    public class TestEventPublisher : IEventPublisher
    {
        public Task Publish<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent
        {
            Published++;
            Token = cancellationToken;
            return Task.CompletedTask;
        }

        public CancellationToken Token { get; set; }
        public int Published { get; private set; }
    }
}
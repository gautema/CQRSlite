using System.Threading.Tasks;

namespace CQRSlite.Events
{
    public interface IEventPublisher
    {
        Task Publish<T>(T @event) where T : IEvent;
    }
}
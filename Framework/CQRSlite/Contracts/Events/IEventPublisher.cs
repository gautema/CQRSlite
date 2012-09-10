using CQRSlite.Contracts.Messages;

namespace CQRSlite.Contracts.Events
{
    public interface IEventPublisher
    {
        void Publish<T>(T @event) where T : Event;
    }
}
using CQRSlite.Contracts.Bus.Messages;

namespace CQRSlite.Contracts.Bus.Events
{
    public interface IEventPublisher
    {
        void Publish<T>(T @event) where T : Event;
    }
}
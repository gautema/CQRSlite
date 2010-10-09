namespace CQRSlite.Eventing
{
    public interface IEventPublisher
    {
        void Publish<T>(T @event) where T : Event;
    }
}
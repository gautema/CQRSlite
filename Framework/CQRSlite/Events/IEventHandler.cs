using CQRSlite.Messages;

namespace CQRSlite.Events
{
    public interface IEventHandler<in T> : IHandler<T> where T : IEvent
    { }
}

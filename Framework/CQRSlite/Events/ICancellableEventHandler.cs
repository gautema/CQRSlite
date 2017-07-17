using CQRSlite.Messages;

namespace CQRSlite.Events
{
    public interface ICancellableEventHandler<in T> : ICancellableHandler<T> where T : IEvent
    {
    }
}
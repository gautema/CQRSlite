using CQRSlite.Messages;

namespace CQRSlite.Commands
{
    public interface ICancellableCommandHandler<in T> : ICancellableHandler<T> where T : ICommand
    {
    }
}
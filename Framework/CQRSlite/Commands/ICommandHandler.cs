using CQRSlite.Messages;

namespace CQRSlite.Commands
{
    public interface ICommandHandler<in T> : IHandler<T> where T : ICommand
    {
    }
}
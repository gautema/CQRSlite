using CQRSlite.Contracts.Messages;

namespace CQRSlite.Contracts.Commands
{
    public interface ICommandSender
    {
        void Send<T>(T command) where T : Command;
    }
}
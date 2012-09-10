using CQRSlite.Contracts.Bus.Messages;

namespace CQRSlite.Contracts.Bus.Commands
{
    public interface ICommandSender
    {
        void Send<T>(T command) where T : Command;
    }
}
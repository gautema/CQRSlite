using System.Threading.Tasks;

namespace CQRSlite.Commands
{
    public interface ICommandSender
    {
        Task Send<T>(T command) where T : ICommand;
    }
}
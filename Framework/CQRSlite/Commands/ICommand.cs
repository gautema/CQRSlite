using CQRSlite.Messages;

namespace CQRSlite.Commands
{
    public interface ICommand : IMessage
    {
        int ExpectedVersion { get; set; }
    }
}
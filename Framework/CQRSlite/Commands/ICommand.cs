using CQRSlite.Messages;

namespace CQRSlite.Commands
{
    /// <summary>
    /// Defines an command with required fields.
    /// </summary>
    public interface ICommand : IMessage
    {
        int ExpectedVersion { get; }
    }
}
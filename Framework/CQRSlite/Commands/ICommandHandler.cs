using CQRSlite.Messages;

namespace CQRSlite.Commands
{
	public interface ICommandHandler<T> : IHandler<T> where T : ICommand
	{
	}
}
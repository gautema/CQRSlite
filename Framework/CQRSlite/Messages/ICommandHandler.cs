using CQRSlite.Commands;

namespace CQRSlite.Messages
{
	public interface ICommandHandler<T> : IHandler<T> where T : Command
	{
	}
}
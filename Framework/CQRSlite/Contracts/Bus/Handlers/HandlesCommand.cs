using CQRSlite.Contracts.Bus.Messages;

namespace CQRSlite.Contracts.Bus.Handlers
{
	public interface HandlesCommand<T> : Handles<T> where T : Command
	{
	}
}
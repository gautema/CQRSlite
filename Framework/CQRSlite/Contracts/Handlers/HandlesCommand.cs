using CQRSlite.Contracts.Messages;

namespace CQRSlite.Contracts.Handlers
{
	public interface HandlesCommand<T> : Handles<T> where T : Command
	{
	}
}
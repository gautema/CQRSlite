using CQRSlite.Contracts.Messages;

namespace CQRSlite.Contracts.Handlers
{
	public interface HandlesEvent<T> : Handles<T> where T : Event
	{
	}
}